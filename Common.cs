using System.Windows;

namespace ES2MD;

/// <summary>
/// Generic helper functions and global variables
/// </summary>
internal static class Common
{
	public static int AnimationSum = 0;
	public static int ArgumentSum = 0;
	public static int ArrayAccessorSum = 0;
	public static int ConditionalSum = 0;
	public static int DialogueSum = 0;
	public static int IdentifierSum = 0;
	public static int LabelSum = 0;
	public static int NodeSum = 0;
	public static int SwitchSum = 0;
	public static int TemplateSum = 0;
	public static int TokenSum = 0;

	public static List<string> Identifiers = [];

	public static void Concurrent(Action callback) => Application.Current.Dispatcher.Invoke(callback);

	public static T Concurrent<T>(Func<T> callback) => Application.Current.Dispatcher.Invoke(callback);
}