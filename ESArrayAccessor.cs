using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// An object of type ESArrayAccessor corresponds to lines of format "$A[b] = C", an array assignment.
/// Embedded array accessors, such as variables used in switch stateents, are handled elsewhere.
/// </summary>
internal partial class ESArrayAccessor : ESToken
{
	public ESArrayAccessor() : base()
	{
	}

	public ESArrayAccessor(string value, int Indent = 0) : base(value, ESTokenType.ArrayAccess, Indent)
	{
	}

	public string GetTargetArray() => ArrayRegex().Match(TokenValue).Groups[1].Value;

	public string GetTargetIndex() => ArrayRegex().Match(TokenValue).Groups[2].Value;

	public string GetTargetValue() => ArrayRegex().Match(TokenValue).Groups[3].Value;

	public override string ToString() => $@"{new string('\t', Indent)}Array accessor
{new string('\t', Indent + 1)}Array: {GetTargetArray()}
{new string('\t', Indent + 1)}Index: {GetTargetIndex()}
{new string('\t', Indent + 1)}Value: {GetTargetValue()}";

	[GeneratedRegex(@"([$\w\.]+)\s*?\[\s*?([\w\.]+)\s*?\]\s+=\s*?([\w\.]+)")]

	private static partial Regex ArrayRegex();
}