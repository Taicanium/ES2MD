using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using static ES2MD.Common;

namespace ES2MD;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private readonly List<ESTree> Trees = [];

	public MainWindow()
	{
		InitializeComponent();
	}

	private static int GetArrayAccessorSum(ESTree tree) => tree.Animations.Sum((ESAnimation anim) => anim.Nodes.Sum(GetArrayAccessorTokens));
	private static int GetArrayAccessorTokens(ESNode node) => node.Tokens.Where(IsArrayAccessToken).Count();
	private static int GetDialogueSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Sum(GetDialogueTokens));
	private static int GetDialogueTokens(ESNode node) => node.Tokens.Where(IsDialogueToken).Count();
	private static int GetNodeSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Count);
	private static int GetSwitchSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Sum(GetSwitchTokens));
	private static int GetSwitchTokens(ESNode node) => node.Tokens.Where(IsSwitchToken).Count();
	private static int GetTemplateSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Sum(GetTemplateTokens));
	private static int GetTemplateTokens(ESNode node) => node.Tokens.Where(IsTemplateToken).Count();
	private static int GetTokenCount(ESNode node) => node.Tokens.Count;
	private static int GetTokenSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Sum(GetTokenCount));
	private static bool IsArrayAccessToken(ESToken token) => token.TokenType == ESToken.ESTokenType.ArrayAccess;
	private static bool IsDialogueToken(ESToken token) => token.TokenType == ESToken.ESTokenType.Dialogue;
	private static bool IsSwitchToken(ESToken token) => token.TokenType == ESToken.ESTokenType.Switch;
	private static bool IsTemplateToken(ESToken token) => token.TokenType == ESToken.ESTokenType.Template;

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
			BackgroundWorker worker = new();
			worker.DoWork += (_, _) =>
			{
				int fileCount = 0;
				foreach (var file in openFileDialog.FileNames)
				{
					Concurrent(() => {
						ProcessESFile(file);
						fileCount++;

						ResultsBox.Text = $@"Files: {fileCount}

Animations: {Trees.Sum(static tree => tree.Animations.Count)}
Array accessors: {Trees.Sum(GetArrayAccessorSum)}
Dialogues: {Trees.Sum(GetDialogueSum)}
Nodes: {Trees.Sum(GetNodeSum)}
Switches: {Trees.Sum(GetSwitchSum)}
Templates: {Trees.Sum(GetTemplateSum)}
Tokens: {Trees.Sum(GetTokenSum)}";
					});
				}
			};

			worker.RunWorkerAsync();
		}
	}

	private ESTree ProcessESFile(string filename)
	{
		string[] lineData = File.ReadAllLines(filename);

		ESTree newTree = new();
		newTree.Construct(string.Join(' ', lineData.Select(line => line.Trim())), Path.GetFileNameWithoutExtension(filename));

		Trees.Add(newTree);

		return newTree;
	}
}