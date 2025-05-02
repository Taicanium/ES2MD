using System.Text.RegularExpressions;
using static ES2MD.Common;

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

		var split = ComplexTreeRegex().Split(fileData);
		
		for (int i = 1; i < split.Length; i += 2)
		{
			var matches = BodyRegex().Matches(split[i + 1]);
			if (matches.Count == 0)
				continue;

			ESAnimation animation = new(matches[0].Groups[1].Value.Trim());

			if (animation.Construct(matches[0].Groups[2].Value.Trim(), split[i].Trim()))
			{
				AnimationSum++;
				Animations.Add(animation);
			}
		}

		return true;
	}

	public bool Construct(string fileData, string name)
	{
		_name = name;
		return Construct(fileData);
	}

	public override string ToString() => $"{string.Join("\n", Animations.Select(anim => anim.ToString()))}";

	[GeneratedRegex(@"def\s*(\d+)\s*(?:for\s*)*")]
	private static partial Regex ComplexTreeRegex();

	[GeneratedRegex(@"\s*(.*?)\{(.+)")]
	private static partial Regex BodyRegex();

	[GeneratedRegex(@"def (\d+)")]
	private static partial Regex IndexRegex();
}