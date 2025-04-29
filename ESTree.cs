using System.Linq;
using System.Text.RegularExpressions;

namespace ES2MD
{
	/// <summary>
	/// An object of type ESTree corresponds to an entire .exps file and all of the animations inside it.
	/// This is the root object created when an EXPS file is opened.
	/// </summary>
	internal class ESTree
	{
		private List<ESAnimation> _animations;

		public List<ESAnimation> Animations { get => _animations; private set => _animations = value; }

		public ESTree()
		{
			_animations = [];
		}

		public bool Construct(string fileData)
		{
			var matches = Regex.Matches(fileData, @"def \d+\s*{(.+)}");

			foreach (Match match in matches)
			{
				ESAnimation animation = new();
				if (animation.Construct(match.Groups[1].Value))
					Animations.Add(animation);
			}

			return true;
		}

		public override string ToString() => $"{string.Join("\n    ", Animations.Select(anim => anim.ToString()))}";
	}
}
