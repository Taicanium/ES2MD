using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private int FileCount = 0;
	private readonly List<ESTree> Trees = [];

	public MainWindow()
	{
		InitializeComponent();
	}

	private void CloseButton(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void OpenButton(object sender, RoutedEventArgs e)
	{
		Trees.Clear();

		OpenFileDialog openFileDialog = new()
		{
			CheckFileExists = true,
			AddToRecent = true,
			DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			Filter = "ExplorerScript files (*.exps)|*.exps|All files (*.*)|*.*",
			Multiselect = true,
			ValidateNames = true,
		};

		if (openFileDialog.ShowDialog() is true)
		{
			BackgroundWorker fileWorker = new();
			BackgroundWorker displayWorker = new()
			{
				WorkerSupportsCancellation = true,
			};

			fileWorker.DoWork += (_, _) =>
			{
				AnimationSum = 0;
				ArgumentSum = 0;
				ArrayAccessorSum = 0;
				ConditionalSum = 0;
				DialogueSum = 0;
				FileCount = 0;
				IdentifierSum = 0;
				LabelSum = 0;
				NodeSum = 0;
				SwitchSum = 0;
				TemplateSum = 0;
				TokenSum = 0;

				foreach (var file in openFileDialog.FileNames)
				{
					Concurrent(() => {
						ProcessESFile(file);
						FileCount++;
					});

					SpinWait.SpinUntil(() => false, 5);
				}
			};

			fileWorker.RunWorkerCompleted += (_, _) =>
			{
				displayWorker.CancelAsync();
			};

			displayWorker.DoWork += (sender, _) =>
			{
				while (((BackgroundWorker?)sender)?.CancellationPending is false)
				{
					Concurrent(() =>
					{
						ResultsBox.Text = $@"Files: {FileCount:N0}

Animations: {AnimationSum:N0}
Arguments: {ArgumentSum:N0}
Array accessors: {ArrayAccessorSum:N0}
Conditionals: {ConditionalSum:N0}
Dialogues: {DialogueSum:N0}
Identifiers: {IdentifierSum:N0}
Labels: {LabelSum:N0}
Nodes: {NodeSum:N0}
Switches: {SwitchSum:N0}
Templates: {TemplateSum:N0}
Tokens: {TokenSum:N0}";
					});

					SpinWait.SpinUntil(() => false, 40); // To prevent overloading the UI by dispatching the results text too quickly.
				}
			};

			fileWorker.RunWorkerAsync();
			displayWorker.RunWorkerAsync();
		}
	}

	private ESTree ProcessESFile(string filename)
	{
		string[] lineData = File.ReadAllLines(filename);

		ESTree newTree = new();
		if (newTree.Construct(string.Join(' ', lineData.Select(line => CommentRegex().Replace(line, string.Empty).Trim())), Path.GetFileNameWithoutExtension(filename)))
			Trees.Add(newTree);

		ProcessToMarkdown(newTree);

		return newTree;
	}

	private static void ProcessToMarkdown(ESTree tree)
	{
		if (!Directory.Exists($"Syntax Trees/"))
			Directory.CreateDirectory($"Syntax Trees/");
		File.WriteAllText($"Syntax Trees/{tree.Name}.txt", $"{tree}");

		MarkdownState mdState = new();

		foreach (ESNode node in tree.Animations[0].Nodes)
			mdState.Progress(node);

		if (!Directory.Exists($"Markdown/"))
			Directory.CreateDirectory($"Markdown/");
		mdState.Export($"Markdown/{tree.Name}.txt");
	}

	[GeneratedRegex(@"//.*")]
	private static partial Regex CommentRegex();
}