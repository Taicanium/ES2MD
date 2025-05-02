using System.Text.RegularExpressions;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// An object of type ESNode corresponds roughly to a line of code in an EXPS file.
/// A single complete command - whether that be a function call, assignment, or operation - is an ESNode.
/// </summary>
internal partial class ESNode
{
	private bool argument = false;
	private int _indent = 0;
	private List<ESToken> _tokens;

	public int Indent { get => _indent; private set => _indent = value; }
	public List<ESToken> Tokens { get => _tokens; private set => _tokens = value; }

	public ESNode()
	{
		_tokens = [];
	}

	public ESNode(int Indent)
	{
		_tokens = [];
		_indent = Indent;
	}

	public bool Parse(string tokenData)
	{
		var matches = SpaceRegex().Matches(tokenData);
		int braceCount = 0;
		int bracketCount = 0;
		string thisData = string.Empty;

		var val = tokenData;

		if (val.Contains("message_SwitchMenu"))
			;

		for (int i = 0; i < val.Length; i++)
		{
			thisData += val[i];
			if (val[i].Equals('{'))
				braceCount++;
			if (val[i].Equals('<'))
				bracketCount++;
			if (val[i].Equals('}'))
			{
				braceCount--;
				if (braceCount == 0 && bracketCount == 0)
				{
					MakeToken(thisData.Trim());
					thisData = string.Empty;
					continue;
				}
			}
			if (val[i].Equals('>'))
			{
				bracketCount--;
				if (braceCount == 0 && bracketCount == 0)
				{
					MakeToken(thisData.Trim());
					thisData = string.Empty;
					continue;
				}
			}
			if (val[i].Equals(';') && braceCount == 0 && bracketCount == 0)
			{
				MakeToken(thisData.Trim());
				thisData = string.Empty;
				continue;
			}
		}

		if (!string.IsNullOrEmpty(thisData.Trim()))
			MakeToken(thisData.Trim());

		return true;
	}

	private void MakeToken(string data)
	{
		if (SwitchRegex().IsMatch(data))
		{
			if (data.Contains("PROCESS_SPECIAL"))
			{
				Tokens.Add(new ESToken($"Jump-switches to-be-implemented", ESToken.ESTokenType.Unknown, Indent));
				SwitchSum++;
				TokenSum++;
				argument = false;
				return;
			}

			Tokens.Add(new ESSwitch(data, Indent));
			SwitchSum++;
			TokenSum++;
			argument = false;
			return;
		}

		if (ConditionalRegex().IsMatch(data))
		{
			Tokens.Add(new ESConditional(data, Indent));
			ConditionalSum++;
			TokenSum++;
			argument = false;
			return;
		}

		if (TemplateRegex().IsMatch(data))
		{
			var groups = TemplateRegex().Match(data).Groups;

			if (groups[1].Success && groups[1].Value.Trim().Length > 0)
				Tokens.Add(new ESToken(groups[1].Value, ESToken.ESTokenType.Identifier, Indent));

			if (groups[2].Success && groups[2].Value.Trim().Length > 0)
				Tokens.Add(new ESToken(groups[2].Value, ESToken.ESTokenType.Identifier, Indent + 1));

			Tokens.Add(new ESTemplate(groups[3].Value, Indent + 1));

			if (groups[4].Success && groups[4].Value.Trim().Length > 0)
				Tokens.Add(new ESToken(groups[4].Value, ESToken.ESTokenType.Argument, Indent + 1));

			TemplateSum++;
			TokenSum++;
			argument = true;
			return;
		}

		if (data.StartsWith('@'))
		{
			Tokens.Add(new ESToken(data, ESToken.ESTokenType.Label, Indent));
			LabelSum++;
			TokenSum++;
			argument = false;
			return;
		}

		if (ArrayRegex().IsMatch(data))
		{
			Tokens.Add(new ESArrayAccessor(data, Indent));
			ArrayAccessorSum++;
			TokenSum++;
			argument = false;
			return;
		}

		var argMatches = ArgumentRegex().Matches(data);
		foreach (Match arg in argMatches)
		{
			var val = arg.Value;
			if (DialogueRegex().IsMatch(val))
			{
				var diagMatch = DialogueRegex().Match(val);
				Tokens.Add(new ESToken(diagMatch.Groups[2].Value, ESToken.ESTokenType.Dialogue, Indent + 1));
				TokenSum++;
				DialogueSum++;
				continue;
			}

			if (argument)
			{
				Tokens.Add(new ESToken(arg.Value, ESToken.ESTokenType.Argument, Indent + 1));
				TokenSum++;
				ArgumentSum++;
				continue;
			}

			Tokens.Add(new ESToken(arg.Value, ESToken.ESTokenType.Identifier, Indent));
			TokenSum++;
			IdentifierSum++;
			argument = true;
		}

		argument = false;
	}

	public override string ToString() => $"{string.Join("\n", Tokens.Select(token => token.ToString()))}";

	[GeneratedRegex(@"[\.\w]+(?:=*""+[^""]+?""+)*")]
	private static partial Regex ArgumentRegex();

	[GeneratedRegex(@"[$\w\.]+\s*?\[\s*?[\w\.]+\s*?\]\s+=\s*?[\w\.]+")]
	private static partial Regex ArrayRegex();

	[GeneratedRegex(@"^(?:elseif|if|else)\s*?\(*.*?\)*")]
	private static partial Regex ConditionalRegex();

	[GeneratedRegex(@"(\w*?)=*?(""+[^""]+?""+)")]
	private static partial Regex DialogueRegex();

	[GeneratedRegex(@"(\S+)")]
	private static partial Regex SpaceRegex();

	[GeneratedRegex(@"[^{}<>\(\)]+witch.+?\{.*?\}")]
	private static partial Regex SwitchRegex();

	[GeneratedRegex(@"(\w*)\(*(\w*)(<.+>)\(*(\w*)")]
	private static partial Regex TemplateRegex();
}