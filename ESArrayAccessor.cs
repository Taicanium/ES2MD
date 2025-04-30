using System.Text.RegularExpressions;

namespace ES2MD
{
	internal partial class ESArrayAccessor : ESToken
	{
		public ESArrayAccessor() : base()
		{
		}

		public ESArrayAccessor(string value) : base(value, ESTokenType.ArrayAccess)
		{
		}

		public ESArrayAccessor(string value, int Indent) : base(value, ESTokenType.ArrayAccess, Indent)
		{
		}

		public string GetTargetArray() => ArrayRegex().Match(TokenValue).Groups[1].Value;

		public string GetTargetIndex() => ArrayRegex().Match(TokenValue).Groups[2].Value;

		public string GetTargetValue() => ArrayRegex().Match(TokenValue).Groups[3].Value;

		public override string ToString() => $@"{new string('\t', Indent)}Array accessor
{new string('\t', Indent + 1)}Array: {GetTargetArray()}
{new string('\t', Indent + 1)}Index: {GetTargetIndex()}
{new string('\t', Indent + 1)}Value: {GetTargetValue()}";

		[GeneratedRegex(@"([$\w]+)\s*?\[\s*?(\w+)\s*?\]\s+=\s*?(\w+)")]

		private static partial Regex ArrayRegex();
	}
}
