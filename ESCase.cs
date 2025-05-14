namespace ES2MD;

/// <summary>
/// Any of the case statements associated with a higher-level switch statement.
/// Like conditionals, these tokens can uniquely contain other complete EXPS commands inside them.
/// </summary>
internal partial class ESCase : ESToken
{
	public ESNode? CaseValue { get; private set; }
	public string CaseVariable { get; set; } = "default";

	public bool PureDialogue;

	public ESCase(string value, bool newMemberCase, int Indent = 0) : base(value, ESTokenType.Case, Indent)
	{
		CaseValue = new(Indent + 1);

		int valIndex = FindVariable(value);

		if (newMemberCase)
			CaseVariable = CaseVariable.Equals("1") ? "Accept" : "Refuse";

		CaseValue.Parse(value[valIndex..].Trim());

		if (CaseValue.Tokens.Count == 1 && CaseValue.Tokens[0].TokenType == ESTokenType.Dialogue)
			PureDialogue = true;
	}
	
	private int FindVariable(string value)
	{
		bool Dialogue = false;
		string thisData = string.Empty;

		for (int i = 0; i < value.Length; i++)
		{
			thisData += value[i];

			if (thisData.EndsWith("case "))
				thisData = string.Empty;

			if (value[i].Equals('"'))
			{
				Dialogue = !Dialogue;
				continue;
			}
			
			if (!Dialogue && value[i].Equals(':'))
			{
				CaseVariable = thisData[..^1].Trim();

				if (CaseVariable.Contains('"'))
				{
					Dialogue = false;
					thisData = string.Empty;

					for (int j = 0; j < CaseVariable.Length; j++)
					{
						if (CaseVariable[j].Equals('"'))
						{
							Dialogue = !Dialogue;
							continue;
						}

						if (Dialogue)
							thisData += CaseVariable[j];
					}

					CaseVariable = thisData;
				}

				return i + 1;
			}
		}

		return -1;
	}

	public override string ToString() => $"{new string('\t', Indent)}Case:\n{new string('\t', Indent + 1)}Value: {CaseVariable}\n{CaseValue}";
}