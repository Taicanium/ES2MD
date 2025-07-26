using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// Any of the standard branching conditional statements, which can uniquely contain entire ESNodes inside.
/// The actual comparison is not handled programmatically, and is stored as a string. (This may change in the future.)
/// </summary>
internal partial class ESConditional : ESToken
{
	public enum ConditionalType
	{
		None,
		If,
		ElseIf,
		Else,
	}

	public ESNode? ConditionalValue { get; set; }

	private string _comparison = string.Empty;
	private ConditionalType? _condition;

	public string Comparison { get => _comparison; private set => _comparison = value; }
	public ConditionalType? Condition { get => _condition; private set => _condition = value; }

	public ESConditional(string value, int indent = 0) : base(value, ESTokenType.Conditional, indent)
	{
		var matches = ConditionRegex().Matches(value.Trim());
		Condition = matches.Count > 0 ? matches[0].Groups[1].Value switch
		{
			"if" => ConditionalType.If,
			"elseif" => ConditionalType.ElseIf,
			"else" => ConditionalType.Else,
			_ => ConditionalType.None,
		} : ConditionalType.None;

		if (Condition.Equals(ConditionalType.None))
			return;

		if (!Condition.Equals(ConditionalType.Else))
		{
			int braceCount = 0;
			var thisData = string.Empty;
			for (int i = 0; i < value.Length; i++)
			{
				var val = value[i];
				thisData += val;
				if (val.Equals('('))
				{
					braceCount++;
					if (braceCount == 1)
						thisData = string.Empty;
				}
				else if (val.Equals(')'))
				{
					braceCount--;
					if (braceCount != 0)
						continue;

					_comparison = thisData[..^1].Trim();
					break;
				}
			}
		}

		var valMatch = ValueRegex().Match(value.Trim());

		ConditionalValue = new(Indent + 1);
		ConditionalValue.Parse(valMatch.Groups[1].Value);
	}
	
	public override string ToString() => $"{new string('\t', Indent)}Conditional: {Condition}\n{(_comparison.Equals(string.Empty) ? string.Empty : $"\n{new string('\t', Indent + 1)}Condition: {Comparison}\n")}{ConditionalValue}";

	[GeneratedRegex(@"^(elseif|if|else)")]
	private static partial Regex ConditionRegex();

	[GeneratedRegex(@"{(.*)}")]
	private static partial Regex ValueRegex();
}
