using System.Text.RegularExpressions;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// This represents a single scene containing multiple animation commands within an ExplorerScript file.
/// An object of type ESAnimation corresponds to, e.g., the "def 0 {" line as well as everything inside of the def node's braces.
/// </summary>
internal partial class ESAnimation
{
	private int _indent = 0;
	private List<ESNode> _nodes;
	private readonly string _target = string.Empty;

	public int Indent { get => _indent; set => _indent = value; }
	public List<ESNode> Nodes { get => _nodes; private set => _nodes = value; }
	public string AnimIndex { get; private set; } = string.Empty;

	public ESAnimation()
	{
		_nodes = [];
	}

	public ESAnimation(int Indent, string Target)
	{
		_indent = Indent;
		_nodes = [];
		_target = Target.Trim();
	}

	public ESAnimation(string Target)
	{
		_nodes = [];
		_target = Target.Trim();
	}

	public bool Construct(string animData)
	{
		var matches = SpaceRegex().Matches(animData);
		int braceCount = 0;
		string thisData = string.Empty;

		foreach (Match match in matches)
		{
			var val = match.Groups[1].Value;
			thisData += val + " ";
			if (val.Contains('{'))
				braceCount += val.AsSpan().Count('{');
			if (val.Contains('}'))
			{
				braceCount -= val.AsSpan().Count('}');
				if (braceCount == 0)
				{
					MakeNode(thisData);
					thisData = string.Empty;
					continue;
				}
			}
			if (val.Contains(';') && braceCount == 0)
			{
				MakeNode(thisData);
				thisData = string.Empty;
				continue;
			}
		}

		return true;
	}

	public bool Construct(string animData, string aIndex)
	{
		AnimIndex = aIndex;
		return Construct(animData);
	}

	private void MakeNode(string data)
	{
		ESNode node = new(Indent + 1);
		if (node.Parse(data))
		{
			Nodes.Add(node);
			NodeSum++;
		}
	}

	public override string ToString() => $@"{new string('\t', Indent)}def {AnimIndex}:{(string.IsNullOrWhiteSpace(_target) ? string.Empty : "\n" + new string('\t', Indent + 1) + "Target: " + _target)}
{string.Join("\n", Nodes.Select(node => node.ToString()))}";

	[GeneratedRegex(@"(\S+)")]
	private static partial Regex SpaceRegex();
}