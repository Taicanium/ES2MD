using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// An EXPS command is composed of multiple syntactic tokens. The function name, its arguments, and return value are all tokens.
/// We only care about certain tokens, insofar as others can be inferred by the structure of the command.
/// </summary>
internal partial class ESToken(string value, ESToken.ESTokenType type, int Indent = 0)
{
	public enum ESTokenType
	{
		Unknown,
		Argument,
		ArrayAccess,
		Case,
		Conditional,
		Dialogue,
		Identifier,
		JumpSwitch,
		Label,
		Loop,
		Switch,
		Template,
		Type,
	}

	private int _indent = Indent;
	private ESTokenType _tokenType = type;
	private string _tokenValue = StripPadding(value);

	public int Indent { get => _indent; set => _indent = value; }
	public ESTokenType TokenType { get => _tokenType; private set => _tokenType = value; }
	public string TokenValue { get => _tokenValue; private set => _tokenValue = value; }

	private static string StripPadding(string value)
	{
		while (PaddingRegex().IsMatch(value.Trim()))
			value = PaddingRegex().Replace(value.Trim(), string.Empty);

		return value.Trim();
	}

	public override string ToString() => $"{new string('\t', Indent)}{TokenType}: {TokenValue}";

	[GeneratedRegex(@"^""|""$")]
	private static partial Regex PaddingRegex();
}