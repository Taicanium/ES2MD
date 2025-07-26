using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// This represents a single scene containing multiple animation defs within an ExplorerScript file.
/// An object of type ESAnimation corresponds to, e.g., the "def 0 {" line as well as everything inside of the def node's braces.
/// </summary>
internal partial class ESAnimation(string Target, int Indent = 0)
{
	private int _indent = Indent;
	private readonly List<ESNode> _nodes = [];
	private readonly string _target = Target.Trim(); // Subsequent animations past "def 0" are usually pointed at a specific character or animation.

	public string AnimIndex { get; private set; } = string.Empty;
	public int Indent { get => _indent; set => _indent = value; }
	public List<ESNode> Nodes { get => _nodes; }

	public bool Construct(string animData, string aIndex = "")
	{
		AnimIndex = aIndex.Trim();
		int braceCount = 0;
		string thisData = string.Empty;

		for (int i = 0; i < animData.Length; i++)
		{
			var val = animData[i];
			thisData += val;
			if (val.Equals('{'))
				braceCount++;
			else if (val.Equals('}'))
			{
				braceCount--;
				if (braceCount != 0)
					continue;

				MakeNode(thisData);
				thisData = string.Empty;
			}
			else if (val.Equals(';') && braceCount == 0)
			{
				MakeNode(thisData);
				thisData = string.Empty;
			}
		}

		return true;
	}

	private void MakeNode(string data)
	{
		ESNode node = new(Indent + 1);

		if (!node.Parse(data))
			return;

		Nodes.Add(node);
		NodeSum++;
	}

	public override string ToString() => $@"{new string('\t', Indent)}def {AnimIndex}:{(string.IsNullOrWhiteSpace(_target) ? string.Empty : "\n" + new string('\t', Indent + 1) + "Target: " + _target)}
{string.Join("\n", Nodes.Select(node => node.ToString()))}";
}