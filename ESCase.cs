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
		var CaseCaptures = CaseRegex().Match(value).Groups;
		CaseValue = new(Indent + 1);

		CaseVariable = CaseCaptures[1].Success ? CaseCaptures[1].Value : "default";

		if (CaseCaptures[2].Success)
			CaseValue.Parse(CaseCaptures[2].Captures[0].Value);
		else if (CaseCaptures[3].Success)
			CaseValue.Parse(CaseCaptures[3].Value);
	}

	public ESCase(string value, int Indent) : base(value, ESTokenType.Case, Indent)
	{
		var CaseCaptures = CaseRegex().Match(value).Groups;
		CaseValue = new(Indent + 1);

		CaseVariable = CaseCaptures[1].Success ? CaseCaptures[1].Value : "default";

		if (CaseCaptures[2].Success)
			CaseValue.Parse(CaseCaptures[2].Captures[0].Value);
		else if (CaseCaptures[3].Success)
			CaseValue.Parse(CaseCaptures[3].Value);
	}

	public override string ToString() => $"{new string('\t', Indent)}Case:\n{new string('\t', Indent + 1)}Value: {CaseVariable}\n{CaseValue}";

	[GeneratedRegex(@"case\s*?(\d+)\s*?:\s*?{(.+?)}|default\s*?:\s*?{(.+?)}")]
	private static partial Regex CaseRegex();
}