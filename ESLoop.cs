using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// 'forever' is the key case for this class.
/// </summary>
internal partial class ESLoop : ESToken
{
	public readonly ESNode content;
	public readonly string condition;

	public ESLoop() : base()
	{
		content = new();
		condition = string.Empty;
	}

	public ESLoop(string value, int Indent = 0) : base(value, ESTokenType.Loop, Indent)
	{
		var match = LoopRegex().Match(value);

		condition = "forever";
		content = new(Indent + 1);

		if (!content.Parse(match.Groups[1].Value))
			content = new();
	}

	[GeneratedRegex(@"forever\s*?{(.+)}")]
	private static partial Regex LoopRegex();

	public override string ToString() => $@"{new string('\t', Indent)}Loop: {condition}
{content}";
}
