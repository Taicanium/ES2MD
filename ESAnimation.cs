using System.Text.RegularExpressions;

namespace ES2MD
{
	/// <summary>
	/// This represents a single scene containing multiple animation commands within an ExplorerScript file.
	/// An object of type ESAnimation corresponds to, e.g., the "def 0 {" line as well as everything inside of the def node's braces.
	/// </summary>
	internal partial class ESAnimation
	{
		private int _indent = 0;
		private List<ESNode> _nodes;

		public int Indent { get => _indent; set => _indent = value; }
		public List<ESNode> Nodes { get => _nodes; private set => _nodes = value; }
		public int AnimIndex { get; private set; } = 0;

		public ESAnimation()
		{
			_nodes = [];
		}

		public ESAnimation(int Indent)
		{
			_indent = Indent;
			_nodes = [];
		}

		public bool Construct(string animData)
		{
			var matches = AnimationRegex().Matches(animData);

			foreach (Match match in matches)
			{
				ESNode node = new(Indent + 1);
				if (node.Construct(match.Groups[1].Value))
					Nodes.Add(node);
			}

			return true;
		}

		public override string ToString() => $@"{new string('\t', Indent)}def {AnimIndex}:
{string.Join("\n", Nodes.Select(node => node.ToString()))}";

		[GeneratedRegex(@"(.*?);")]
		private static partial Regex AnimationRegex();
	}
}
