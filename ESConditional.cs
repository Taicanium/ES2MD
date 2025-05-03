using System.Text.RegularExpressions;

namespace ES2MD
{
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

		private string? _comparison;
		private ConditionalType? _condition;

		public string? Comparison { get => _comparison; private set => _comparison = value; }
		public ConditionalType? Condition { get => _condition; private set => _condition = value; }

		public ESConditional() : base()
		{
		}

		public ESConditional(string value, int indent = 0) : base(value, ESTokenType.Conditional, indent)
		{
			var matches = ComparisonRegex().Matches(value.Trim());
			Comparison = matches.Count > 0 ? matches[0].Groups[1].Value.Trim() : null;

			matches = ConditionRegex().Matches(value.Trim());
			Condition = matches.Count > 0 ? matches[0].Groups[1].Value switch
			{
				"if" => ConditionalType.If,
				"elseif" => ConditionalType.ElseIf,
				"else" => ConditionalType.Else,
				_ => ConditionalType.None,
			} : null;

			var valMatch = ValueRegex().Match(value.Trim());

			ConditionalValue = new(Indent + 1);
			ConditionalValue.Parse(valMatch.Groups[1].Value);
		}
		
		public override string ToString() => $@"{new string('\t', Indent)}Conditional
{new string('\t', Indent + 1)}Type: {Condition}";

		[GeneratedRegex(@"^(elseif|if|else)")]
		private static partial Regex ConditionRegex();

		[GeneratedRegex(@"\((.+)\)")]
		private static partial Regex ComparisonRegex();
		[GeneratedRegex(@"{(.*)}")]
		private static partial Regex ValueRegex();
	}
}
