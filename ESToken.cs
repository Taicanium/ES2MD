namespace ES2MD
{
	/// <summary>
	/// An EXPS command is composed of multiple syntactic tokens. The function name, its arguments, and return value are all tokens.
	/// We only care about certain tokens, insofar as others can be inferred by the structure of the command.
	/// </summary>
	internal class ESToken
	{
		public enum ESTokenType
		{
			Unknown,
			Argument,
			ArrayAccess,
			Dialogue,
			Identifier,
			Template,
			Type,
		}

		private ESTokenType _tokenType;
		private string _tokenValue;

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
		}

		public override string ToString() => $"{TokenType}: {TokenValue}";
	}
}
