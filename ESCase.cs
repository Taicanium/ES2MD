using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// Any of the case statements associated with a higher-level switch statement.
/// Like conditionals, these tokens can uniquely contain other complete EXPS commands inside them.
/// </summary>
internal partial class ESCase : ESToken
{
	public ESNode CaseValue { get; set; }
	public string CaseVariable { get; set; } = string.Empty;

	public int MenuDepth = 0;

	public bool PureDialogue = false;

	public ESCase() : base()
	{
		CaseValue = new();
		CaseVariable = "default";
	}

	public ESCase(string value, int Indent = 0, int menuDepth = 0) : base(value, ESTokenType.Case, Indent)
	{
		CaseValue = new(Indent + 1);
		var valGroups = ValueRegex().Match(value).Groups;
		var varGroups = VariableRegex().Match(value
			.Replace("menu({", string.Empty)
			.Replace("english=\"\"\"", string.Empty)
			.Replace("english=\"", string.Empty)
			.Replace("\"\"\", })", string.Empty)
			.Replace("\", })", string.Empty)
			.Replace(", })", string.Empty)
			.Replace("\"\"\",})", string.Empty)
			.Replace("\",})", string.Empty)
			.Replace(",})", string.Empty)
			.Replace("})", string.Empty)
			.Trim()).Groups;

		CaseVariable = varGroups[1].Value.Trim();
		if (string.IsNullOrWhiteSpace(CaseVariable))
			CaseVariable = "default";

		if (CaseVariable.Contains("O-OK!"))
			Console.Read();

		MenuDepth = menuDepth;

		CaseValue.Parse(valGroups[1].Value.Trim(), MenuDepth);

		if (CaseValue.Tokens.Count == 1 && CaseValue.Tokens[0].TokenType == ESTokenType.Dialogue)
			PureDialogue = true;
	}

	public override string ToString() => $"{new string('\t', Indent)}Case:\n{new string('\t', Indent + 1)}Value: {CaseVariable}\n{CaseValue}";

	[GeneratedRegex(@"(?:case|default)*[\w\s\p{Po}\p{Pd}]*[\s"",}\)]*:(.+)")]
	private static partial Regex ValueRegex();

	[GeneratedRegex(@"(?:case|default)*([\w\s\p{Po}\p{Pd}]*)[\s"",}\)]*:")]
	private static partial Regex VariableRegex();
}