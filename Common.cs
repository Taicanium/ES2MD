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
	public static int LoopSum = 0;
	public static int NodeSum = 0;
	public static int SwitchSum = 0;
	public static int TemplateSum = 0;
	public static int TokenSum = 0;


	public static string MarkdownFolder = $"./Markdown/";
	public static string SyntaxTreeFolder = $"./Syntax Trees/";

	/// <summary>
	/// DIspatches an action to the main thread for synchronous execution.
	/// </summary>
	public static void Concurrent(Action callback) => Application.Current.Dispatcher.Invoke(callback);

	/// <summary>
	/// Dispatches a function with no arguments to the main thread for synchronous execution.
	/// </summary>
	public static T Concurrent<T>(Func<T> callback) => Application.Current.Dispatcher.Invoke(callback);
}