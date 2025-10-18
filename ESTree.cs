using System.IO;
using System.Text.RegularExpressions;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// An object of type ESTree corresponds to an entire .exps file and all of the animations inside it.
/// This is the root object created when an EXPS file is opened.
/// </summary>
partial class ESTree()
{
	private readonly List<ESAnimation> _animations = [];
	private string _name = string.Empty;

	public List<ESAnimation> Animations => _animations;
	public string Name { get => _name; private set => _name = value; }

	public bool Construct(string fileData)
	{
		_name = string.IsNullOrWhiteSpace(_name) ? IndexRegex().Match(fileData).Groups[1].Value : _name;

		var split = ComplexTreeRegex().Split(fileData);

		for (int i = 1; i < split.Length; i += 2)
		{
			var matches = BodyRegex().Matches(split[i + 1]);
			if (matches.Count == 0)
				continue;

			var target = matches[0].Groups[1].Value.Trim();
			ESAnimation animation = new(string.IsNullOrWhiteSpace(target) ? null : target);

			if (!animation.Construct(matches[0].Groups[2].Value.Trim(), split[i].Trim()))
				continue;

			AnimationSum++;
			Animations.Add(animation);
		}

		return true;
	}

	public bool Construct(string fileData, string name)
	{
		_name = name;
		return Construct(fileData);
	}

	public void ProcessToMarkdown()
	{
		if (!Directory.Exists(SyntaxTreeFolder))
			Directory.CreateDirectory(SyntaxTreeFolder);

		if (!Directory.Exists(MarkdownFolder))
			Directory.CreateDirectory(MarkdownFolder);

		File.WriteAllText($"{SyntaxTreeFolder}{Name}.txt", $"{this}");

		foreach (ESAnimation anim in Animations)
		{
			string? target = anim.Target ?? string.Empty;
			Localization.actors.TryGetValue(target, out target);
			MarkdownState mdState = new(target);

			foreach (ESNode node in anim.Nodes)
				mdState.Progress(node);

			if (!Directory.Exists($"{MarkdownFolder}{Name}/"))
				Directory.CreateDirectory($"{MarkdownFolder}{Name}/");

			mdState.Export($"{MarkdownFolder}{Name}/{anim.AnimIndex}.md");

			if (new FileInfo($"{MarkdownFolder}{Name}/{anim.AnimIndex}.md").Length == 0)
				File.Delete($"{MarkdownFolder}{Name}/{anim.AnimIndex}.md");
		}

		if (new DirectoryInfo($"{MarkdownFolder}{Name}/").EnumerateFiles().Sum(info => info.Length) == 0)
			Directory.Delete($"{MarkdownFolder}{Name}/", true);
	}

	public override string ToString() => $"{string.Join("\n", Animations.Select(anim => anim.ToString().Replace($"\n\n", $"\n")))}";

	[GeneratedRegex(@"def\s*(\d+)\s*(?:for\s*)*")]
	private static partial Regex ComplexTreeRegex();

	[GeneratedRegex(@"\s*(.*?)\{(.+)")]
	private static partial Regex BodyRegex();

	[GeneratedRegex(@"def (\d+)")]
	private static partial Regex IndexRegex();
}