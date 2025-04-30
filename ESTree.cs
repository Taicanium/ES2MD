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
		_name = Regex.Match(fileData, @"def (\d+)").Groups[1].Value;

		var matches = TreeRegex().Matches(fileData);

		foreach (Match match in matches)
		{
			ESAnimation animation = new();
			if (animation.Construct(match.Groups[1].Value))
				Animations.Add(animation);
		}

		return true;
	}

	public bool Construct(string fileData, string name)
	{
		_name = name;

		var matches = TreeRegex().Matches(fileData);

		foreach (Match match in matches)
		{
			ESAnimation animation = new();
			if (animation.Construct(match.Groups[1].Value))
				Animations.Add(animation);
		}

		return true;
	}

	public override string ToString() => $"{string.Join("\n", Animations.Select(anim => anim.ToString()))}";

	[GeneratedRegex(@"def \d+\s*{(.+)}")]
	private static partial Regex TreeRegex();
}