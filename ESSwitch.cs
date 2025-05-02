using System.Text.RegularExpressions;

namespace ES2MD;

internal partial class ESSwitch : ESToken
{
	public List<ESCase> Cases { get; } = [];

	public ESSwitch() : base()
	{

	}

	public ESSwitch(string value) : base(value, ESTokenType.Switch)
	{
		var CaseMatches = CaseRegex().Matches(TokenValue);

		foreach (Match Case in CaseMatches)
		{
			Cases.Add(new(Case.Value));
		}
	}

	public ESSwitch(string value, int Indent) : base(value, ESTokenType.Switch, Indent)
	{
		var thisData = string.Empty;
		int braceCount = 0;

		for (int i = 1; i < value.Length; i++)
		{
			thisData += value[i];
			if (value[i].Equals('{'))
			{
				braceCount++;
				if (braceCount == 1)
					thisData = string.Empty;
			}
			if (value[i].Equals('}'))
			{
				braceCount--;
				if (braceCount == 1)
				{
					Cases.Add(new(thisData, Indent + 1));
					thisData = string.Empty;
				}
				if (braceCount == 0)
					return;
			}
		}
	}

	public string GetTargetVariable()
	{
		return VariableRegex().Match(TokenValue).Groups[1].Value.Trim();
	}

	public string GetCases()
	{
		return $"{string.Join('\n', Cases)}";
	}

	public override string ToString()
	{
		return $"{new string('\t', Indent)}Switch:\n{new string('\t', Indent + 1)}Variable: {GetTargetVariable()}\n{GetCases()}";
	}

	[GeneratedRegex(@"((case|default).*?{.*?})(?=case|default)*")]
	private static partial Regex CaseRegex();

	[GeneratedRegex(@"\((\s*.+?\s*)\)\s*?{")]
	private static partial Regex VariableRegex();
}