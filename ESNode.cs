using System.Text.RegularExpressions;

namespace ES2MD
{
	/// <summary>
	/// An object of type ESNode corresponds roughly to a line of code in an EXPS file.
	/// A single complete command - whether that be a function call, assignment, or operation - is an ESNode.
	/// </summary>
	internal class ESNode
	{
		private List<ESToken> _tokens;

		public List<ESToken> Tokens { get => _tokens; private set => _tokens = value; }

		public ESNode()
		{
			_tokens = [];
		}

		public bool Construct(string nodeData)
		{
			var matches = Regex.Matches(nodeData, @"(<*?\w+>*?)");
			var hasID = false;

			foreach (Match match in matches)
			{
				var value = match.Groups[1].Value;
				Tokens.Add(value.Contains('<') ? new ESTemplate(value, ESToken.ESTokenType.Template) : new(value, hasID ? ESToken.ESTokenType.Argument : ESToken.ESTokenType.Identifier));
				hasID = true;
			}

			return true;
		}

		public override string ToString() => $"{string.Join("\n            ", Tokens.Select(token => token.ToString()))}";
	}
}
