using System.Text.RegularExpressions;

namespace ES2MD
{
	/// <summary>
	/// An object of type ESNode corresponds roughly to a line of code in an EXPS file.
	/// A single complete command - whether that be a function call, assignment, or operation - is an ESNode.
	/// </summary>
	internal partial class ESNode
	{
		private List<ESToken> _tokens;

		public List<ESToken> Tokens { get => _tokens; private set => _tokens = value; }

		public ESNode()
		{
			_tokens = [];
		}

		public bool Construct(string nodeData)
		{
			var matches = NodeRegex().Matches(nodeData);
			var hasID = false;

			foreach (Match match in matches)
			{
				Tokens.Add(match.Groups[1].Success ? new ESTemplate(match.Groups[1].Value) :
					match.Groups[2].Success ? new ESToken(match.Groups[2].Value, ESToken.ESTokenType.Dialogue) :
					match.Groups[3].Success ? new ESArrayAccessor(match.Groups[3].Value) :
					new ESToken(match.Groups[4].Value, hasID ? ESToken.ESTokenType.Argument : ESToken.ESTokenType.Identifier));
				hasID = true;
			}

			return true;
		}

		public override string ToString() => $"{string.Join("\n            ", Tokens.Select(token => token.ToString()))}";
		[GeneratedRegex(@"<(.*?)>|""(.+)""|([$\w]+\s*?\[\s*?\w+\s*?\]\s*?=\s*?\w+)|(\w+)")]
		private static partial Regex NodeRegex();
	}
}
