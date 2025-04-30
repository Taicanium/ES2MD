using Microsoft.Win32;
using System.IO;
using System.Windows;

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

	private void CloseButton(object sender, RoutedEventArgs e)
	{
		foreach (ESTree tree in Trees)
			File.WriteAllText($"{tree.Name}.txt", $"{tree}");

		Application.Current.Shutdown();
	}

	private void OpenButton(object sender, RoutedEventArgs e)
	{
		static int GetArrayAccessorSum(ESTree tree) => tree.Animations.Sum((ESAnimation anim) => anim.Nodes.Sum((ESNode node) => node.Tokens.Where((ESToken token) => token.TokenType == ESToken.ESTokenType.ArrayAccess).Count()));
		static int GetDialogueSum(ESTree tree) => tree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Where(token => token.TokenType == ESToken.ESTokenType.Dialogue).Count()));
		static int GetNodeSum(ESTree tree) => tree.Animations.Sum(static anim => anim.Nodes.Count);
		static int GetTemplateSum(ESTree tree) => tree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Where(token => token.TokenType == ESToken.ESTokenType.Template).Count()));
		static int GetTokenSum(ESTree tree) => tree.Animations.Sum(anim => anim.Nodes.Sum(node => node.Tokens.Count));

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
			foreach (var file in openFileDialog.FileNames)
				ProcessESFile(file);

		ResultsBox.Text = $@"Animations: {Trees.Sum(static tree => tree.Animations.Count)}
Nodes: {Trees.Sum(GetNodeSum)}
Templates: {Trees.Sum(GetTemplateSum)}
Dialogues: {Trees.Sum(GetDialogueSum)}
Array accessors: {Trees.Sum(GetArrayAccessorSum)}
Tokens: {Trees.Sum(GetTokenSum)}";
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