using System.Text.RegularExpressions;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// An object of type ESNode corresponds roughly to a line of code in an EXPS file.
/// A single complete command - whether that be a function call, assignment, or operation - is an ESNode.
/// </summary>
internal partial class ESNode
{
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

		foreach (Match match in matches)
		{
			var val = match.Groups[1].Value;
			thisData += val + " ";
			if (val.Contains('{'))
				braceCount += val.AsSpan().Count('{');
			if (val.Contains('<'))
				bracketCount += val.AsSpan().Count('<');
			if (val.Contains("}"))
			{
				braceCount -= val.AsSpan().Count('}');
				if (braceCount == 0)
				{
					MakeToken(thisData.Trim());
					thisData = string.Empty;
					continue;
				}
			}
			if (val.Contains(">"))
			{
				bracketCount -= val.AsSpan().Count('>');
				if (bracketCount == 0)
				{
					MakeToken(thisData.Trim());
					thisData = string.Empty;
					continue;
				}
			}
			if (val.Contains(";") && braceCount == 0)
			{
				MakeToken(thisData.Trim().Replace(";", string.Empty));
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
			Tokens.Add(new ESSwitch(data, Indent));
			SwitchSum++;
			TokenSum++;
			return;
		}

		if (TemplateRegex().IsMatch(data))
		{
			Tokens.Add(new ESTemplate(TemplateRegex().Match(data).Groups[1].Value, Indent + 1));
			TemplateSum++;
			TokenSum++;
			return;
		}

		if (data.StartsWith("@"))
		{
			Tokens.Add(new ESToken(data, ESToken.ESTokenType.Label, Indent));
			LabelSum++;
			TokenSum++;
			return;
		}

		if (ConditionalRegex().IsMatch(data))
		{
			Tokens.Add(new ESConditional(data, Indent));
			ConditionalSum++;
			TokenSum++;
			return;
		}

		if (ArrayRegex().IsMatch(data))
		{
			Tokens.Add(new ESArrayAccessor(data, Indent));
			ArrayAccessorSum++;
			TokenSum++;
			return;
		}

		var argMatches = ArgumentRegex().Matches(data);
		bool argument = false;
		foreach (Match arg in argMatches)
		{
			var val = arg.Value;
			if (DialogueRegex().IsMatch(val))
			{
				var diagMatch = DialogueRegex().Match(val);
				Tokens.Add(new ESToken(diagMatch.Groups[2].Value, ESToken.ESTokenType.Dialogue, Indent + 1));
				TokenSum++;
				DialogueSum++;
				return;
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
	}

	public override string ToString() => $"{string.Join("\n", Tokens.Select(token => token.ToString()))}";

	[GeneratedRegex(@"\w+(?:=*""+[^""]+?""+)*")]
	private static partial Regex ArgumentRegex();

	[GeneratedRegex(@"[$\w\.]+\s*?\[\s*?[\w\.]+\s*?\]\s+=\s*?[\w\.]+")]
	private static partial Regex ArrayRegex();

	[GeneratedRegex(@"^(?:elseif|if|else)\s*?\(*.*?\)*")]
	private static partial Regex ConditionalRegex();

	[GeneratedRegex(@"(\w*?)=*?(""+[^""]+?""+)")]
	private static partial Regex DialogueRegex();

	[GeneratedRegex(@"(\S+)")]
	private static partial Regex SpaceRegex();

	[GeneratedRegex(@"^[^{}<>\(\)]+witch.+")]
	private static partial Regex SwitchRegex();

	[GeneratedRegex(@"(<.+>)")]
	private static partial Regex TemplateRegex();
}