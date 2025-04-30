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
			File.WriteAllText("lastTree.txt", $"{tree}");

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
			Multiselect = false,
			ValidateNames = true,
		};

		if (openFileDialog.ShowDialog() is true)
			foreach (var file in openFileDialog.FileNames)
				ProcessESFile(file);
	}

	private void ProcessESFile(string filename)
	{
		string[] lineData = File.ReadAllLines(filename);

		ESTree newTree = new();
		newTree.Construct(string.Join(' ', lineData.Select(line => line.Trim())));

		Trees.Add(newTree);

		ResultsBox.Text = $@"Animations: {newTree.Animations.Count}
Nodes: {newTree.Animations.Sum(static anim => anim.Nodes.Count)}
Templates: {newTree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Where(token => token.TokenType == ESToken.ESTokenType.Template).Count()))}
Dialogues: {newTree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Where(token => token.TokenType == ESToken.ESTokenType.Dialogue).Count()))}
Array accessors: {newTree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Where(token => token.TokenType == ESToken.ESTokenType.ArrayAccess).Count()))}
Tokens: {newTree.Animations.Sum(static anim => anim.Nodes.Sum(static node => node.Tokens.Count))}";
	}
}