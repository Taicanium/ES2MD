using System.IO;
using System.Text.RegularExpressions;

namespace ES2MD;

static partial class EXPSUtils
{
	public static readonly List<ESTree> Trees = [];

	public static ESTree ProcessESFile(string filename)
	{
		string[] lineData = File.ReadAllLines(filename);
		ESTree newTree = new();

		try
		{
			if (newTree.Construct(string.Join(' ', lineData.Select(line => CommentRegex().Replace(line, string.Empty).Trim())), Path.GetFileNameWithoutExtension(filename)))
				Trees.Add(newTree);

			newTree.ProcessToMarkdown();
		}
		catch
		{
			return newTree;
		}

		return newTree;
	}

	[GeneratedRegex(@"//.*")]
	private static partial Regex CommentRegex();
}
