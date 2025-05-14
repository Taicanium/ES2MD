using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// An object of type ESArrayAccessor corresponds to lines of format "$A[b] = C", an array assignment.
/// Embedded array accessors, such as variables used in switch stateents, are handled elsewhere.
/// </summary>
internal partial class ESArrayAccessor(string value, int Indent = 0) : ESToken(value, ESTokenType.ArrayAccess, Indent)
{
	private string GetTargetArray() => ArrayRegex().Match(TokenValue).Groups[1].Value;

	private string GetTargetIndex() => ArrayRegex().Match(TokenValue).Groups[2].Value;

	private string GetTargetValue() => ArrayRegex().Match(TokenValue).Groups[3].Value;

	public override string ToString() => $@"{new string('\t', Indent)}Array accessor
{new string('\t', Indent + 1)}Array: {GetTargetArray()}
{new string('\t', Indent + 1)}Index: {GetTargetIndex()}
{new string('\t', Indent + 1)}Value: {GetTargetValue()}";

	[GeneratedRegex(@"([$\w\.]+)\s*?\[\s*?([\w\.]+)\s*?\]\s+=\s*?([\w\.]+)")]
	private static partial Regex ArrayRegex();
}