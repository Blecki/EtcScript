using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EtcScriptEmu
{
	public partial class Main : Form
	{
		private EtcScriptLib.Environment ScriptEnvironment;
		private EtcScriptLib.SyntaxLex SyntaxLex;
		private Dictionary<EtcScriptLib.SyntaxLex.TokenStyle, int> StyleMap;

		private class OpenFile
		{
			public String Filename;
			public String Text;
			public bool Changes;
			public ToolStripItem TabButton;
		}

		private List<OpenFile> OpenFiles = new List<OpenFile>();
		private OpenFile CurrentFile = null;

		public Main()
		{
			InitializeComponent();

			//scintilla1.Styles[0].ForeColor = Color.Black;
			//scintilla1.Styles[1].ForeColor = Color.Red;
			//scintilla1.Styles[2].ForeColor = Color.Green;
			//scintilla1.Styles[3].ForeColor = Color.Blue;
			//scintilla1.Styles[4].ForeColor = Color.Orange;

			StyleMap = new Dictionary<EtcScriptLib.SyntaxLex.TokenStyle, int>();
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Basic, 0);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Clause, 4);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.ComplexStringPart, 2);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Error, 1);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Keyword, 3);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Number, 2);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.Operator, 4);
			StyleMap.Add(EtcScriptLib.SyntaxLex.TokenStyle.String, 2);

			ScriptEnvironment = new EtcScriptLib.Environment();
			SyntaxLex = new EtcScriptLib.SyntaxLex(ScriptEnvironment.Context, (token, style, fold) =>
				{
					if (StyleMap.ContainsKey(style))
						StyleToken(token, StyleMap[style], fold);
					else
						StyleToken(token, 0, fold);

				},
				(token) =>
				{
					var range = new ScintillaNET.Range(token.Location.Index, token.Location.EndIndex, scintilla1);
					range.StartingLine.IsFoldPoint = true;
				});
		}

		private void oPENToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				var openFile = OpenFiles.FirstOrDefault(of => of.Filename == fileDialog.FileName);
				if (openFile == null)
				{
					openFile = new OpenFile {
						Filename = fileDialog.FileName,
						Text = System.IO.File.ReadAllText(fileDialog.FileName),
						Changes = false,
						TabButton = toolStrip1.Items.Add(System.IO.Path.GetFileName(fileDialog.FileName))
					};
					OpenFiles.Add(openFile);
					openFile.TabButton.Click += (s, args) => {
						SwitchToFile(openFile);	
					};
				}

				SwitchToFile(openFile);
			}
		}

		private void SwitchToFile(OpenFile newFile)
		{
			if (CurrentFile != null)
				CurrentFile.Text = scintilla1.Text;
			CurrentFile = newFile;
			scintilla1.Text = CurrentFile.Text;
			this.Text = CurrentFile.Filename;
		}

		private void scintilla1_StyleNeeded(object sender, ScintillaNET.StyleNeededEventArgs e)
		{
			//textBox1.Text += e.Range.Start.ToString() + " - " + e.Range.End.ToString() + "\r\n";
			
			//try
			//{
			//    var stringIterator = new EtcScriptLib.StringIterator(e.Range.Text);
			//    var tokenIterator = new EtcScriptLib.TokenStream(stringIterator, ScriptEnvironment.Context);
			//    SyntaxLex.Style(tokenIterator);
			//}
			//catch (Exception ex) { }
			//scintilla1.Folding.IsEnabled = true;
			//e.Range.SetStyle(1);
			//e.Range.StartingLine.IsFoldPoint = true;
			//e.Range.StartingLine.FoldLevel = e.Range.StartingLine.VisibleLineNumber;
		}

		private void StyleToken(EtcScriptLib.Token Token, int Style, int FoldLevel)
		{
			var range =	new ScintillaNET.Range(Token.Location.Index, Token.Location.EndIndex, scintilla1);
			range.SetStyle(Style);
			//range.StartingLine.FoldLevel = FoldLevel;
			////range.StartingLine.Indentation = FoldLevel;
			//if (FoldLevel > this.FoldLevel)
			//    range.StartingLine.IsFoldPoint = true;
			//else
			//    range.StartingLine.IsFoldPoint = false;
			//this.FoldLevel = FoldLevel;
		}

		private void cOMPILEToolStripMenuItem_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
			ScriptEnvironment = new EtcScriptLib.Environment();
			var hadError = false;
			ScriptEnvironment.Build(scintilla1.Text, (error) =>
				{
					textBox1.Text += error + "\r\n";
					hadError = true;
					return EtcScriptLib.ErrorStrategy.Abort;
				});

			treeView1.Nodes.Clear();
			if (!hadError)
			{
				var macroNode = treeView1.Nodes.Add("MACROS");
				foreach (var macro in ScriptEnvironment.Context.TopScope.Macros)
					macroNode.Nodes.Add(macro.DescriptiveHeader);
				var ruleNode = treeView1.Nodes.Add("RULEBOOKS");
				foreach (var rulebook in ScriptEnvironment.Context.Rules.Rulebooks)
				{
					var rulebookNode = ruleNode.Nodes.Add(rulebook.ToString());
					foreach (var rule in rulebook.Rules)
						rulebookNode.Nodes.Add(rule.DescriptiveHeader);
				}
			}
			
		}

	}
}
