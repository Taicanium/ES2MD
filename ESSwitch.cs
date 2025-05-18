using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// Switch statements allow for easier selection of actions to take among many options assignable to a single variable.
/// Each option is handled as an ESCase.
/// </summary>
internal partial class ESSwitch : ESToken
{
	public List<ESCase> Cases { get; } = [];

	private readonly string[] BreakWords = ["break;", "continue;", "end;"];

	public bool MarkedDown { get; set; } = false;

	public ESSwitch(string value, int Indent = 0) : base(value, ESTokenType.Switch, Indent)
	{
		List<bool> braceLevels = [];
		bool init = false;
		bool newMemberMenu = GetTargetVariable().Contains("MENU_ACCEPT_TEAM_MEMBER", StringComparison.InvariantCultureIgnoreCase);
		int orderCount = 0;
		bool quote = false;
		string thisData = string.Empty;
		bool waitingOnCase = true;

		for (int i = 2; i < value.Length; i++)
		{
			thisData += value[i];
			if (!init && !value[i].Equals('{'))
				continue;

			if (!init)
			{
				init = true;
				orderCount++;
				thisData = string.Empty;
			}

			if (value[i].Equals('"'))
				quote = !quote;

			if (quote)
				continue;

			if (value[i].Equals(':') && !thisData.EndsWith(": default:") && !value[i..(i + 6)].Equals(": case"))
			{
				orderCount++;
				waitingOnCase = false;

				while (braceLevels.Count < orderCount + 1)
					braceLevels.Add(false);

				braceLevels[orderCount] = false;
			}

			if (waitingOnCase)
				continue;

			if (value[i].Equals('{') && !value[i - 2].Equals(':'))
			{
				orderCount++;
				while (braceLevels.Count < orderCount + 1)
					braceLevels.Add(false);

				braceLevels[orderCount] = true;
			}

			if (value[i].Equals('}') || (!braceLevels[orderCount] && (JumpRegex().IsMatch(thisData) || BreakWords.Any(word => thisData.EndsWith(word)))))
			{
				orderCount--;
				if (orderCount == 1)
				{
					var dcReg = DoubleCaseRegex().Replace(thisData, ":");
					Cases.Add(new(dcReg.Replace(": default:", ":").Trim(), newMemberMenu, Indent + 1));
					thisData = string.Empty;
					waitingOnCase = true;
				}
			}
		}
	}

	public string GetTargetVariable() => VariableRegex().Match(TokenValue).Groups[1].Value.Trim();

	private string GetCases() => $"{string.Join('\n', Cases)}";

	public override string ToString() => $"{new string('\t', Indent)}Switch:\n{new string('\t', Indent + 1)}Variable: {GetTargetVariable()}\n{GetCases()}";

	[GeneratedRegex(@"jump @*[\w_]+;$")]
	private static partial Regex JumpRegex();

	[GeneratedRegex(@"\((\s*.+?\s*)\)\s*?{")]
	private static partial Regex VariableRegex();

	[GeneratedRegex(@"(: case .*?)+:")]
	private static partial Regex DoubleCaseRegex();
}