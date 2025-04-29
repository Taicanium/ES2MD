namespace ES2MD
{
	/// <summary>
	/// A template is stored in an EXPS file within angle brackets (<>) - they indicate a specific target that a function is applied to.
	/// For instance, Turn2Direction is a function that causes an actor to turn a certain direction. Which specific actor is turning is listed within a template.
	/// </summary>
	internal class ESTemplate : ESToken
	{
		public ESTemplate() : base()
		{
		}

		public ESTemplate(string value, ESTokenType type) : base(value, type)
		{
		}

		public string GetTargetType() => TokenValue.Split(' ')[0];

		public string GetTargetIdentifier() => TokenValue.Split(' ')[1];
	}
}
