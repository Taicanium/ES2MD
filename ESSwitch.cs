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
			Cases.Add(new(Case.Value, Indent + 1));
		}
	}

	public ESSwitch(string value, int Indent) : base(value, ESTokenType.Switch, Indent)
	{
		var CaseMatches = CaseRegex().Matches(TokenValue);

		foreach (Match Case in CaseMatches)
		{
			Cases.Add(new(Case.Value, Indent + 1));
		}
	}

	public string GetTargetVariable()
	{
		Match match = VariableRegex().Match(TokenValue);
		return match.Value;
	}

	public string GetCases()
	{
		return $"{string.Join('\n', Cases)}";
	}

	public override string ToString()
	{
		return $"{new string('\t', Indent)}Switch:\n{new string('\t', Indent + 1)}Variable: {GetTargetVariable()}\n{GetCases()}";
	}

	[GeneratedRegex(@"case\s*?\d+\s*?:\s*?.+?}|default\s*?:\s*?.+?}")]
	private static partial Regex CaseRegex();

	[GeneratedRegex(@"\$[\w\.]+")]
	private static partial Regex VariableRegex();
}