using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// An EXPS command is composed of multiple syntactic tokens. The function name, its arguments, and return value are all tokens.
/// We only care about certain tokens, insofar as others can be inferred by the structure of the command.
/// </summary>
internal partial class ESToken
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
		Label,
		Switch,
		Template,
		Type,
	}

	private int _indent = 0;
	private ESTokenType _tokenType;
	private string _tokenValue;

	public int Indent { get => _indent; set => _indent = value; }
	public ESTokenType TokenType { get => _tokenType; private set => _tokenType = value; }
	public string TokenValue { get => _tokenValue; private set => _tokenValue = value; }

	public ESToken()
	{
		_tokenType = ESTokenType.Unknown;
		_tokenValue = string.Empty;
	}

	public ESToken(string value, ESTokenType type)
	{
		_tokenType = type;
		_tokenValue = value;

		if (type == ESTokenType.Identifier)
			Common.Identifiers.Add(value);
	}

	public ESToken(string value, ESTokenType type, int Indent)
	{
		_tokenType = type;
		_tokenValue = value;
		_indent = Indent;

		if (type == ESTokenType.Identifier)
			Common.Identifiers.Add(value);
	}

	private static string StripPadding(string value)
	{
		while (PaddingRegex().IsMatch(value.Trim()))
			value = PaddingRegex().Replace(value.Trim(), string.Empty);

		return value.Trim();
	}

	public override string ToString() => $"{new string('\t', Indent)}{TokenType}: {StripPadding(TokenValue)}";
	[GeneratedRegex(@"^""|""$")]
	private static partial Regex PaddingRegex();
}