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
	private string? Effect;
	private string? EffectActor;
	private string? Face;
	private readonly Dictionary<string, string> Faces = [];
	private string? Identifier;
	private readonly List<string> History = [];

	private void AddHistory(string input)
	{
		string bullets = string.Empty;
		for (int i = 0; i < CaseDepth; i++)
			bullets += "  * ";
		History.Add($"{bullets}{input}");
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
				AddHistory($"*Jump to [anchor {label}](#{label})*");
				History.Add(string.Empty);
				History.Add(string.Empty);
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
				{
					AddHistory($"{Effect}");
					History.Add(string.Empty);
				}
				break;
		}
	}

	private void ProcessDialogue(string input)
	{
		if (Identifier?.Equals("message_Mail") is true)
		{
			if (blockQuote)
				History[^1] += ">";
			AddHistory($">{ProcessTags(input.Trim())}");
			History.Add(string.Empty);
			blockQuote = true;
			return;
		}

		if (Actor is not null && Face is not null)
		{
			AddHistory($"`{Actor.Replace($" Name", string.Empty)} {Face}`");
			if (Effect is not null)
				History[^1] += $" {Effect}";
			History.Add(string.Empty);
		}
		else if (Effect is not null)
		{
			AddHistory($"{Effect}");
			History.Add(string.Empty);
		}

		var afterTags = ProcessTags(input.Trim());
		if (!afterTags.Equals(string.Empty))
		{
			AddHistory($"`{Actor ?? "💬"}`: \"{afterTags}\"");
			History.Add(string.Empty);
			History.Add(string.Empty);
		}

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
				break;
		}
	}

	private bool ProcessSwitchCase(ESCase Case)
	{
		if (Case.MenuDepth != CaseDepth)
		{
			AddHistory($"*If the player chooses \"{Case.CaseVariable}\":*");
			History.Add(string.Empty);
		}

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
				AddHistory($"*Anchor: <a name=\"{label}\"></a>{label}*");
				History.Add(string.Empty);
				History.Add(string.Empty);
				break;
			case ESToken.ESTokenType.Loop:
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
				AddHistory($"[//]: # ({input.TokenValue})");
				History.Add(string.Empty);
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
