using System.IO;
using System.Text.RegularExpressions;
using static ES2MD.Localization;

namespace ES2MD;

/// <summary>
/// The Markdown state engine handles translation of an EXPS file to Markdown.
/// </summary>
internal partial class MarkdownState
{
	private string? Actor;
	private int ArgumentIndex = 0;
	private bool blockQuote = false;
	private int CaseDepth = 0;
	private int ConditionalDepth = 0;
	private string? Effect;
	private string? EffectActor;
	private string? Face;
	private bool Faded;
	private readonly Dictionary<string, string> Faces = [];
	private string? Identifier;
	private readonly List<string> History = [];
	private int LoopDepth = 0;

	private void AddHistory(string input, int blankLines)
	{
		string bullets = string.Empty;
		for (int i = 0; i < CaseDepth + LoopDepth + ConditionalDepth; i++)
			bullets += "  * ";
		History.Add($"{bullets}{input}");
		for (int i = 0; i < blankLines; i++)
			History.Add(string.Empty);
	}

	private static string AssertLabel(string input) => input.Replace("@", string.Empty).Replace("label", string.Empty).Replace("_", string.Empty).Replace(";", string.Empty);

	public bool Export(string filename)
	{
		File.WriteAllLines(filename, History);
		Reset();
		return true;
	}

	private void ProcessArgument(string argument)
	{
		switch (Identifier)
		{
			case "jump":
				var label = AssertLabel(argument);
				AddHistory($"*Jump to [anchor {label}](#{label})*", 2);
				break;
			case "message_SetActor":
				Actor = null;
				if (actors.TryGetValue(argument, out Actor))
					Faces.TryGetValue(Actor, out Face);
				break;
			case "message_SetFace":
			case "message_SetFaceOnly":
				if (ArgumentIndex == 0)
				{
					if (!actors.TryGetValue(argument, out Actor))
						Actor = null;
					break;
				}
				
				if (ArgumentIndex == 1)
				{
					Face = null;
					if (faces.TryGetValue(argument, out Face) && Actor is not null)
						Faces[Actor] = Face;
					break;
				}

				break;
			case "SetEffect":
				if (int.TryParse(argument, out var _))
					break;

				var OldEffect = Effect;
				if (!effects.TryGetValue(argument, out Effect))
					Effect = OldEffect;

				if (argument.Equals("EFFECT_NONE"))
					Effect = null;

				if (EffectActor is null && Effect is not null)
					AddHistory($"{Effect}", 1);
				break;
		}
	}

	private void ProcessDialogue(string input)
	{
		if (Identifier?.Equals("message_Mail") is true)
		{
			if (blockQuote)
				History[^1] += ">";
			History.Add($">{ProcessTags(input.Trim())}");
			History.Add(string.Empty);
			blockQuote = true;
			return;
		}

		if (Actor is not null && Face is not null)
		{
			AddHistory($"`{Actor.Replace($" Name", string.Empty)} {Face}`", 1);
			if (Effect is not null)
				History[^2] += $" {Effect}";
		}
		else if (Effect is not null)
			AddHistory($"{Effect}", 1);

		var afterTags = ProcessTags(input.Trim());
		if (!afterTags.Equals(string.Empty))
			AddHistory($"`{Actor ?? "💬"}`: \"{afterTags}\"", 2);

		blockQuote = false;
		Effect = null;
		EffectActor = null;
	}

	private void ProcessIdentifier()
	{
		switch (Identifier)
		{
			case "message_ResetActor":
				Actor = null;
				Face = null;
				Faded = false;
				break;
			case "screen_FadeIn":
			case "screen_FadeInAll":
			case "screen2_FadeIn":
			case "screen2_FadeInAll":
			case "screen_FadeOut":
			case "screen_FadeOutAll":
			case "screen2_FadeOut":
			case "screen2_FadeOutAll":
			case "screen_WhiteOut":
				if (!Faded)
					AddHistory("* * *", 2);
				Faded = true;
				break;
			default:
				Faded = false;
				break;
		}
	}

	private bool ProcessSwitchCase(ESCase Case)
	{
		if (Case.MenuDepth != CaseDepth)
			AddHistory($"*If the player chooses \"{Case.CaseVariable}\":*", 1);

		int depthTemp = CaseDepth;
		CaseDepth = Case.MenuDepth;

		if (!Case.PureDialogue || Case.CaseVariable.Equals("default"))
		{
			for (int i = 0; i < Case.CaseValue.Tokens.Count; i++)
				if (!Progress(Case.CaseValue.Tokens[i]))
					return false;
		}

		CaseDepth = depthTemp;
		return true;
	}

	private static string ProcessTags(string input)
	{
		foreach (KeyValuePair<string, string> tag in tags)
			input = input.Replace($"[{tag.Key}]", tag.Value);

		foreach (Match match in TagRegex().Matches(input))
			input = input.Replace(match.Value, string.Empty);

		return input;
	}

	private bool Progress(ESToken input)
	{
		switch (input.TokenType)
		{
			case ESToken.ESTokenType.ArrayAccess:
				break;
			case ESToken.ESTokenType.Argument:
				ProcessArgument(input.TokenValue);
				ArgumentIndex++;
				return true;
			case ESToken.ESTokenType.Case:
				break;
			case ESToken.ESTokenType.Conditional:
				var cInput = (ESConditional)input;
				if (cInput.Condition == ESConditional.ConditionalType.Else)
					AddHistory("*Else:*", 2);
				else if (cInput.Comparison.Contains("SCENARIO_MAIN_BIT_FLAG"))
				{
					if (cInput.Comparison.Contains("[33]"))
						AddHistory("*If any of the seven treasures has been collected:*", 2);
					else if (cInput.Comparison.Contains("[48]"))
						AddHistory("*If the player has received Sneasel's gift:*", 2);
					else if (cInput.Comparison.Contains("[8]"))
						AddHistory("*If the game was last saved by sleeping:*", 2);
					else
						AddHistory("*If a certain flag is set:*", 2);
				}
				else if (cInput.Comparison.Contains("PERFORMANCE_PROGRESS_LIST[7]"))
					AddHistory("*If the party leader cannot be switched at this time:*", 2);
				else if (cInput.Comparison.Contains("not debug"))
					AddHistory("*If not debugging:*", 2);
				else if (cInput.Comparison.Contains("debug"))
					AddHistory("*If debugging:*", 2);
				else if (cInput.Comparison.Contains("SCENARIO_") &&
					cInput.Comparison.Contains(">=") || cInput.Comparison.Contains("=="))
					AddHistory("*If the player has progressed far enough:*", 2);
				else if (cInput.Comparison.Contains("SCENARIO_") && cInput.Comparison.Contains('<'))
					AddHistory("*If the player has not progressed far enough:*", 2);
				else
					AddHistory("*If certain conditions are met:*", 2);
				ConditionalDepth++;
				if (!Progress(cInput.ConditionalValue))
					return false;
				ConditionalDepth--;
				break;
			case ESToken.ESTokenType.Dialogue:
				ProcessDialogue(input.TokenValue.Replace("\\", string.Empty));
				break;
			case ESToken.ESTokenType.Identifier:
				Identifier = input.TokenValue;
				ProcessIdentifier();
				break;
			case ESToken.ESTokenType.Label:
				var label = AssertLabel(input.TokenValue);
				AddHistory($"*Anchor: <a name=\"{label}\"></a>{label}*", 2);
				break;
			case ESToken.ESTokenType.Loop:
				var lInput = (ESLoop)input;
				AddHistory("*Loop forever:*", 2);
				LoopDepth++;
				if (!Progress(lInput.content))
					return false;
				LoopDepth--;
				break;
			case ESToken.ESTokenType.Switch:
				var sInput = (ESSwitch)input;

				if (sInput.GetTargetVariable().Contains("PROCESS_SPECIAL_GET_HERO_KIND"))
				{
					if (!ProcessSwitchCase(sInput.Cases[0]))
						return false;
					break;
				}

				foreach (ESCase Case in sInput.Cases)
					if (!ProcessSwitchCase(Case))
						return false;
				break;
			case ESToken.ESTokenType.Template:
				if (Identifier?.Equals("SetEffect") is true)
				{
					var identifiers = ((ESTemplate)input).GetTargetIdentifiers(false);
					if (identifiers.Length > 0)
						actors.TryGetValue(identifiers[0], out EffectActor);
				}
				break;
			case ESToken.ESTokenType.Type:
				break;
			default:
				AddHistory($"[//]: # ({input.TokenValue})", 1);
				break;
		}

		ArgumentIndex = 0;
		return true;
	}

	public bool Progress(ESNode input)
	{
		bool state = true;
		foreach (ESToken token in input.Tokens)
			state = state && Progress(token);

		return state;
	}

	public bool Reset()
	{
		History.Clear();

		return true;
	}

	[GeneratedRegex(@"\[[\w:]+\]")]
	private static partial Regex TagRegex();
}
