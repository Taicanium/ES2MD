using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// Switch statements allow for easier selection of actions to take among many options assignable to a single variable.
/// Each option is handled as an ESCase.
/// </summary>
internal partial class ESSwitch : ESToken
{
	public List<ESCase> Cases { get; } = [];

	public int MenuDepth = 0;

	public ESSwitch() : base()
	{

	}

	public ESSwitch(string value, int Indent = 0, int menuDepth = 0) : base(value, ESTokenType.Switch, Indent)
	{
		bool colonBrace = false;
		MenuDepth = menuDepth;
		bool newMemberMenu = GetTargetVariable().Contains("MENU_ACCEPT_TEAM_MEMBER", StringComparison.InvariantCultureIgnoreCase);
		int orderCount = 0;
		bool quote = false;
		var thisData = string.Empty;
		bool waitingForCase = false;

		if (GetTargetVariable().Contains("menu", StringComparison.InvariantCultureIgnoreCase))
			MenuDepth++;

		for (int i = 5; i < value.Length; i++)
		{
			thisData += value[i];

			if (value[i].Equals('"'))
			{
				colonBrace = false;
				quote = !quote;
				continue;
			}

			if (thisData.EndsWith("case"))
				waitingForCase = true;

			if ((value[i].Equals('{') && !colonBrace && !waitingForCase) || (!quote && value[i].Equals(':') && (i > value.Length - 7 || !value[(i + 2)..].StartsWith("case")) && (i > value.Length - 10 || !value[(i + 2)..].StartsWith("default"))))
			{
				colonBrace = true;
				orderCount++;

				if (!quote && value[i].Equals(':'))
					waitingForCase = false;

				if (orderCount == 1)
					thisData = string.Empty;

				continue;
			}

			if (!waitingForCase && (value[i].Equals('}') || thisData.EndsWith("break;") || thisData.EndsWith("end;") || JumpRegex().IsMatch(thisData)))
			{
				colonBrace = false;
				orderCount--;

				if (orderCount == 0)
					return;

				if (orderCount == 1)
				{
					Cases.Add(new(DoubleCaseRegex().Replace(thisData.Replace(": default:", ":"), ":"),
						newMemberMenu,
						Indent + 1, MenuDepth));
					thisData = string.Empty;
				}

				continue;
			}

			if (char.IsLetterOrDigit(value[i]))
				colonBrace = false;
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

	[GeneratedRegex(@"jump @*\w+;$")]
	private static partial Regex JumpRegex();

	[GeneratedRegex(@"\((\s*.+?\s*)\)\s*?{")]
	private static partial Regex VariableRegex();

	[GeneratedRegex(@": case .*?:")]
	private static partial Regex DoubleCaseRegex();
}