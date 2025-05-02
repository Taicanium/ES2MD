using System.Text.RegularExpressions;

namespace ES2MD;

internal partial class ESCase : ESToken
{
	public ESNode CaseValue { get; set; }
	public string CaseVariable { get; set; } = string.Empty;

	public ESCase() : base()
	{
		CaseValue = new();
		CaseVariable = "default";
	}

	public ESCase(string value) : base(value, ESTokenType.Case)
	{
	}

	public ESCase(string value, int Indent) : base(value, ESTokenType.Case, Indent)
	{
		CaseValue = new(Indent + 1);
		var valGroups = ValueRegex().Match(value).Groups;
		var varGroups = VariableRegex().Match(value).Groups;

		CaseVariable = varGroups[1].Value.Trim();
		if (string.IsNullOrEmpty(CaseVariable))
			CaseVariable = "default";

		CaseValue.Parse(valGroups[1].Value.Trim());
	}

	public override string ToString() => $"{new string('\t', Indent)}Case:\n{new string('\t', Indent + 1)}Value: {CaseVariable}\n{CaseValue}";

	[GeneratedRegex(@"{(.+)}")]
	private static partial Regex ValueRegex();

	[GeneratedRegex(@"(\w+):")]
	private static partial Regex VariableRegex();
}