using System.Text.RegularExpressions;

namespace ES2MD
{
	/// <summary>
	/// This represents a single scene containing multiple animation commands within an ExplorerScript file.
	/// An object of type ESAnimation corresponds to, e.g., the "def 0 {" line as well as everything inside of the def node's braces.
	/// </summary>
	internal partial class ESAnimation
	{
		private List<ESNode> _nodes;

		public List<ESNode> Nodes { get => _nodes; private set => _nodes = value; }

		public ESAnimation()
		{
			_nodes = [];
		}

		public bool Construct(string animData)
		{
			var matches = AnimationRegex().Matches(animData);

			foreach (Match match in matches)
			{
				ESNode node = new();
				if (node.Construct(match.Groups[1].Value))
					Nodes.Add(node);
			}

			return true;
		}

		public override string ToString() => $"{string.Join("\n        ", Nodes.Select(node => node.ToString()))}";
		[GeneratedRegex(@"(.*?);")]
		private static partial Regex AnimationRegex();
	}
}
