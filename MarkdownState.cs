using System.IO;
using System.Text.RegularExpressions;
using static ES2MD.Localization;

namespace ES2MD
{
	internal partial class MarkdownState
	{
		private string? Actor;
		private int ArgumentIndex = 0;
		private bool blockQuote = false;
		private string? Effect;
		private string? EffectActor;
		private string? Face;
		private readonly Dictionary<string, string> Faces = [];
		private string? Identifier;
		private readonly List<string> History = [];

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
				case "message_SetActor":
					Actor = null;
					if (actors.TryGetValue(argument, out Actor))
						Faces.TryGetValue(Actor, out Face);
					break;
				case "SetEffect":
					if (!effects.TryGetValue(argument, out Effect))
						Effect = null;

					if (EffectActor is not null)
						History[^2] += $"{Effect}";
					break;
				case "message_SetFace":
				case "message_SetFaceOnly":
					if (ArgumentIndex == 0)
					{
						if (!actors.TryGetValue(argument, out Actor))
							Actor = null;
						break;
					}
					else if (ArgumentIndex == 1)
					{
						Face = null;
						if (faces.TryGetValue(argument, out Face) && Actor is not null)
							Faces[Actor] = Face;
						break;
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
				History.Add($">{ProcessTags(input.Trim())}");
				History.Add(string.Empty);
				blockQuote = true;
				return;
			}

			if (Actor is not null && Face is not null)
			{
				History.Add($"`{Actor.Replace($" Name", string.Empty)} {Face}`");
				if (Effect is not null)
					History[^1] += $" {Effect}";
			}
			else if (Effect is not null)
				History.Add($"{Effect}");

			if (Actor is not null)
				History.Add($"`{Actor}`");
			else
				History.Add($"💬");

			History[^1] += $": \"{ProcessTags(input.Trim())}\"";
			History.Add(string.Empty);

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
				case ESToken.ESTokenType.Argument:
					ProcessArgument(input.TokenValue);
					ArgumentIndex++;
					return true;
				case ESToken.ESTokenType.Dialogue:
					ProcessDialogue(input.TokenValue);
					break;
				case ESToken.ESTokenType.Identifier:
					Identifier = input.TokenValue;
					ProcessIdentifier();
					break;
				case ESToken.ESTokenType.Switch:
					var sInput = (ESSwitch)input;
					var cases = sInput.Cases;
					if (cases.Count > 0 && cases[0].CaseValue.Tokens.Count > 0)
						ProcessDialogue(cases[0].CaseValue.Tokens[0].TokenValue);
					break;
				case ESToken.ESTokenType.Template:
					if (Identifier?.Equals("SetEffect") is true)
					{
						var identifiers = ((ESTemplate)input).GetTargetIdentifiers(false);
						if (identifiers.Length > 0 && actors.TryGetValue(identifiers[0], out EffectActor))
						{
							History.Add($"`{EffectActor}`: ");
							History.Add(string.Empty);
						}
					}
					break;
				default:
					History.Add($"[//]: # ({input.TokenValue})");
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
}
