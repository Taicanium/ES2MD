using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// 'forever' is the key case for this class.
/// </summary>
internal partial class ESLoop : ESToken
{
	private readonly ESNode? _content;
	private readonly string _condition = string.Empty;

	public ESNode? Content { get => _content; }

	public ESLoop(string value, int Indent = 0) : base(value, ESTokenType.Loop, Indent)
	{
		var match = LoopRegex().Match(value);

		_condition = "forever";
		_content = new(Indent + 1);

		if (!_content.Parse(match.Groups[1].Value))
			_content = new();
	}

	[GeneratedRegex(@"forever\s*?{(.+)}")]
	private static partial Regex LoopRegex();

	public override string ToString() => $@"{new string('\t', Indent)}Loop: {_condition}
{_content}";
}
