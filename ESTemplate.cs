using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// A template is stored in an EXPS file within angle brackets (<>) - they indicate a specific target that a function is applied to.
/// For instance, Turn2Direction is a function that causes an actor to turn a certain direction. Which specific actor is turning is listed within a template.
/// </summary>
internal partial class ESTemplate : ESToken
{
	public ESTemplate() : base()
	{
	}

	public ESTemplate(string value) : base(value, ESTokenType.Template)
	{
	}

	public ESTemplate(string value, int Indent) : base(value, ESTokenType.Template, Indent)
	{
	}

	public string GetTargetType()
	{
		var TokenList = TemplateRegex().Matches(TokenValue);
		return TokenList.ElementAt(0).Groups[1].Value;
	}

	public string[] GetTargetIdentifiers()
	{
		var TokenList = TemplateRegex().Matches(TokenValue).ToList();
		return [.. TokenList.GetRange(1, TokenList.Count - 1).Select(match => match.Groups[1].Value).Select(value => $"\n{new string('\t', Indent + 1)}Identifier: {value}")];
	}

	public override string ToString() => $@"{new string('\t', Indent)}Template
{new string('\t', Indent + 1)}Type: {GetTargetType()}{string.Concat(GetTargetIdentifiers())}";

	[GeneratedRegex(@"([\.\w]+)")]
	private static partial Regex TemplateRegex();
}