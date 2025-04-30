using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// An object of type ESTree corresponds to an entire .exps file and all of the animations inside it.
/// This is the root object created when an EXPS file is opened.
/// </summary>
internal partial class ESTree
{
	private List<ESAnimation> _animations;
	private string _name;

	public List<ESAnimation> Animations { get => _animations; private set => _animations = value; }
	public string Name { get => _name; private set => _name = value; }

	public ESTree()
	{
		_animations = [];
		_name = string.Empty;
	}

	public bool Construct(string fileData)
	{
		_name = string.IsNullOrWhiteSpace(_name) ? IndexRegex().Match(fileData).Groups[1].Value : _name;

		var matches1 = SimpleTreeRegex().Matches(fileData);
		var matches2 = ComplexTreeRegex().Matches(fileData);

		foreach (Match match in matches1)
		{
			ESAnimation animation = new();
			if (animation.Construct(match.Groups[2].Value, match.Groups[1].Value))
				Animations.Add(animation);
		}

		foreach (Match match in matches2)
		{
			ESAnimation animation = new(match.Groups[2].Value);
			if (animation.Construct(match.Groups[3].Value, match.Groups[1].Value))
				Animations.Add(animation);
		}

		return true;
	}

	public bool Construct(string fileData, string name)
	{
		_name = name;
		return Construct(fileData);
	}

	public override string ToString() => $"{string.Join("\n", Animations.Select(anim => anim.ToString()))}";

	[GeneratedRegex(@"def\s*?(\d+)\s*?for\s*?(.+?){(.+)}")]
	private static partial Regex ComplexTreeRegex();

	[GeneratedRegex(@"def\s*?(\d+)\s*?{(.+)}")]
	private static partial Regex SimpleTreeRegex();
	[GeneratedRegex(@"def (\d+)")]
	private static partial Regex IndexRegex();
}