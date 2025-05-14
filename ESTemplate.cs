using System.Text.RegularExpressions;

namespace ES2MD;

/// <summary>
/// A template is stored in an EXPS file within angle brackets (<>) - they indicate a specific target that a function is applied to.
/// For instance, Turn2Direction is a function that causes an actor to turn a certain direction. Which specific actor is turning is listed within a template.
/// </summary>
internal partial class ESTemplate(string value, int Indent = 0) : ESToken(value, ESTokenType.Template, Indent)
{
	public string[] GetTargetIdentifiers(bool display = true)
	{
		var TokenList = TemplateRegex().Matches(TokenValue).ToList();

		if (display)
			return [.. TokenList.GetRange(1, TokenList.Count - 1).Select(match => match.Groups[1].Value).Select(value => $"\n{new string('\t', Indent + 1)}Identifier: {value}")];

		return [.. TokenList.GetRange(1, TokenList.Count - 1).Select(match => match.Groups[1].Value)];
	}

	private string GetTargetType() => TemplateRegex().Matches(TokenValue).ElementAt(0).Groups[1].Value;

	public override string ToString() => $@"{new string('\t', Indent)}Template
{new string('\t', Indent + 1)}Type: {GetTargetType()}{string.Concat(GetTargetIdentifiers())}";

	[GeneratedRegex(@"([\.\w]+)")]
	private static partial Regex TemplateRegex();
}