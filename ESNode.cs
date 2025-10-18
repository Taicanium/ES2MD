using System.Text.RegularExpressions;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// An object of type ESNode corresponds roughly to a line of code in an EXPS file.
/// A single complete command - whether that be a function call, assignment, operation, or action performed by an actor - is an ESNode.
/// </summary>
internal partial class ESNode(int Indent = 0)
{
	private bool _argument = false;
	private int _indent = Indent;
	private List<ESToken> _tokens = [];

	public int Indent { get => _indent; private set => _indent = value; }
	public List<ESToken> Tokens { get => _tokens; private set => _tokens = value; }

	public bool Parse(string tokenData)
	{
		int braceCount = 0;
		string thisData = string.Empty;
		var val = tokenData;

		for (int i = 0; i < val.Length; i++)
		{
			thisData += val[i];
			if (val[i].Equals('{'))
				braceCount++;
			else if (val[i].Equals('}'))
			{
				braceCount--;
				if (braceCount != 0)
					continue;

				MakeToken(thisData.Trim());
				thisData = string.Empty;
			}
			else if (val[i].Equals(';') && braceCount == 0)
			{
				MakeToken(thisData.Trim());
				thisData = string.Empty;
			}
		}

		if (!string.IsNullOrWhiteSpace(thisData.Trim()))
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
			_argument = false;
			return;
		}

		if (ConditionalRegex().IsMatch(data))
		{
			Tokens.Add(new ESConditional(data, Indent));
			ConditionalSum++;
			TokenSum++;
			_argument = false;
			return;
		}

		if (data.StartsWith('@'))
		{
			Tokens.Add(new ESToken(data, ESToken.ESTokenType.Label, Indent));
			LabelSum++;
			TokenSum++;
			_argument = false;
			return;
		}

		if (LoopRegex().IsMatch(data))
		{
			Tokens.Add(new ESLoop(data, Indent));
			TokenSum++;
			LoopSum++;
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
				foreach (Match argMatch in SubtemplateRegex().Matches(groups[4].Value.Trim()))
					Tokens.Add(new ESToken(argMatch.Value.Trim(), ESToken.ESTokenType.Argument, Indent + 1));

			TemplateSum++;
			TokenSum++;
			_argument = false;
			return;
		}

		if (ArrayRegex().IsMatch(data))
		{
			Tokens.Add(new ESArrayAccessor(data, Indent));
			ArrayAccessorSum++;
			TokenSum++;
			_argument = false;
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

			if (_argument)
			{
				Tokens.Add(new ESToken(arg.Value, ESToken.ESTokenType.Argument, Indent + 1));
				TokenSum++;
				ArgumentSum++;
				continue;
			}

			Tokens.Add(new ESToken(arg.Value, ESToken.ESTokenType.Identifier, Indent));
			TokenSum++;
			IdentifierSum++;
			_argument = true;
		}

		_argument = false;
	}

	public override string ToString() => $"{string.Join("\n", Tokens.Select(token => token.ToString()))}";

	[GeneratedRegex(@"[\.\w]+(?:=*""+.+""+)*")]
	private static partial Regex ArgumentRegex();

	[GeneratedRegex(@"[$\w\.]+\s*?\[\s*?[\w\.]+\s*?\]\s+=\s*?[\w\.]+")]
	private static partial Regex ArrayRegex();

	[GeneratedRegex(@"^(?:elseif|if|else)(\s*?\(*.*?\)*)*?(?=elseif|if|else|$)")]
	private static partial Regex ConditionalRegex();

	[GeneratedRegex(@"(\w*?)=*?(""+.+""+)")]
	private static partial Regex DialogueRegex();

	[GeneratedRegex(@"forever\s*{.+}")]
	private static partial Regex LoopRegex();

	[GeneratedRegex(@"((?:[\w\s'\.]+<[\w\s',\.]+?>)|(?:[\w\s'\.]+))")]
	private static partial Regex SubtemplateRegex();

	[GeneratedRegex(@"^[^{}<>\(\)]+witch.+?\{.*?\}")]
	private static partial Regex SwitchRegex();

	[GeneratedRegex(@"(\w*)\(*(\w*)\)*(<.+?>)\(*([\w\s<>',\.]*)\)*")]
	private static partial Regex TemplateRegex();
}