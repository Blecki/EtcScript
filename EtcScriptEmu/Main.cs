using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ScintillaNET;

namespace EtcScriptEmu
{
	public partial class Main : Form
	{
		private EtcScriptLib.Environment ScriptEnvironment;
		private EtcScriptLib.SyntaxLex SyntaxLex;
		private Dictionary<EtcScriptLib.SyntaxLex.TokenStyle, int> StyleMap;

		private EtcScriptLib.IExternalHost ExternalHost;
		private String ExternalHostFilename;
		private System.Windows.Forms.Form HostForm;

		private bool EnableChangeDetection = true;
		private Settings Settings;

		private class OpenFile
		{
			public String Filename;
			public String Text;
			public bool Changes;
			public FileTab TabButton;
			public bool UnsavedNewFile = false;
		}

		private List<OpenFile> OpenFiles = new List<OpenFile>();
		private OpenFile CurrentFile = null;

		public Main()
		{
			InitializeComponent();

			try
			{
				Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(
					System.IO.File.ReadAllText("default-settings.txt"));
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				Settings = new EtcScriptEmu.Settings();
			}

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

			runButton.Enabled = false;

			var defaultHost = Settings.Hosts.Where(hs => hs.DisplayName == Settings.DefaultHost).FirstOrDefault();
			if (defaultHost != null) PrepareHost(System.IO.Path.GetFullPath(defaultHost.Path));

			if (Settings.OpenFiles.Count == 0) OpenNewFile();
			else foreach (var filename in Settings.OpenFiles)
				Open(filename);
		}

		private void OpenNewFile()
		{
			var newFile = PrepareFileTab("untitled", "");
			newFile.UnsavedNewFile = true;
			SwitchToFile(newFile);
		}

		private void Open(String Filename)
		{
			try
			{
				var openFile = OpenFiles.FirstOrDefault(of => of.Filename == Filename);
				if (openFile == null)
					openFile = PrepareFileTab(Filename, System.IO.File.ReadAllText(Filename));
				if (CurrentFile != null && CurrentFile.UnsavedNewFile && CurrentFile.Changes == false)
				{
					OpenFiles.Remove(CurrentFile);
					openFilesBar.Items.Remove(CurrentFile.TabButton);
				}
				SwitchToFile(openFile);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + " while opening " + Filename);
			}
		}

		private void oPENToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			var fileDialog = new OpenFileDialog();
			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				Open(fileDialog.FileName);
		}

		private void SwitchToFile(OpenFile newFile)
		{
			EnableChangeDetection = false;
			if (CurrentFile != null)
				CurrentFile.Text = scintilla1.Text;
			CurrentFile = newFile;
			scintilla1.Text = CurrentFile.Text;
			this.Text = CurrentFile.Filename;

			foreach (var file in OpenFiles)
				file.TabButton.CurrentTab = false;
			CurrentFile.TabButton.CurrentTab = true;
			EnableChangeDetection = true;
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
			var hadError = false;

			if (ExternalHost == null)
			{
				ScriptEnvironment = new EtcScriptLib.Environment();
				ScriptEnvironment.Build(scintilla1.Text, (error) =>
					{
						textBox1.Text += error + "\r\n";
						hadError = true;
						return EtcScriptLib.ErrorStrategy.Abort;
					});
			}
			else
			{
				if (HostForm != null)
				{
					HostForm.Dispose();
					HostForm = null;
				}

				if (CurrentFile.UnsavedNewFile) sAVEASToolStripMenuItem_Click(null, null);
				if (CurrentFile.Changes) sAVEToolStripMenuItem_Click(null, null);

				ExternalHost.Compile(ExternalHostFilename, CurrentFile.Filename, (error) =>
					{
						if (textBox1.InvokeRequired) textBox1.Invoke(
							new Action(() => { textBox1.Text += error + "\r\n"; }));
						hadError = true;
					},
					(environment) =>
					{
						ScriptEnvironment = environment;
						if (hadError) this.Invoke(new Action(() => treeView1.Nodes.Clear()));
						else this.Invoke(new Action(() => BuildCompileTree()));
					});

			}
		}

		private void BuildCompileTree()
		{
			treeView1.Nodes.Clear();
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
		private void hOSTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			textBox1.Text = "";
			var hadError = false;

			if (ExternalHost == null)
			{
				ScriptEnvironment = new EtcScriptLib.Environment();
				ScriptEnvironment.Build(scintilla1.Text, (error) =>
				{
					textBox1.Text += error + "\r\n";
					hadError = true;
					return EtcScriptLib.ErrorStrategy.Abort;
				});
			}
			else
			{
				if (HostForm != null)
				{
					HostForm.Dispose();
					HostForm = null;
				}

				if (CurrentFile.UnsavedNewFile) sAVEASToolStripMenuItem_Click(null, null);
				if (CurrentFile.Changes) sAVEToolStripMenuItem_Click(null, null);

				var hostControl = ExternalHost.Host(ExternalHostFilename, CurrentFile.Filename, (error) =>
				{
					if (textBox1.InvokeRequired) textBox1.Invoke(
						new Action(() => { textBox1.Text += error + "\r\n"; }));
					else
						textBox1.Text += error + "\r\n";

					hadError = true;
				},
					(environment) =>
					{
						ScriptEnvironment = environment;
						if (hadError) this.Invoke(new Action(() => treeView1.Nodes.Clear()));
						else this.Invoke(new Action(() => BuildCompileTree()));
					});

				HostForm = new Form();
				hostControl.Dock = DockStyle.Fill;
				HostForm.Controls.Add(hostControl);
				HostForm.ClientSize = ExternalHost.PreferredSize();
				HostForm.Show();
			}
		}

		private bool PrepareHost(String Filename)
		{
			try
			{
				var assembly = System.Reflection.Assembly.LoadFile(Filename);
				foreach (var possibleHost in assembly.GetExportedTypes())
					foreach (var implementedInterface in possibleHost.GetInterfaces())
						if (implementedInterface == typeof(EtcScriptLib.IExternalHost))
						{
							ExternalHost = Activator.CreateInstance(possibleHost) as EtcScriptLib.IExternalHost;
							ExternalHostFilename = Filename;
							runButton.Enabled = ExternalHost.SupportsHosting();
							hostLabel.Text = ExternalHost.Name();
							return true;
						}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				return false;
			}
			return false;
		}

		private void sETHOSTToolStripMenuItem_Click(object sender, EventArgs e)
		{
			runButton.Enabled = false;
			var fileDialog = new OpenFileDialog();
			if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				if (!PrepareHost(fileDialog.FileName))
					MessageBox.Show("No host was found.");
		}

		private bool SaveAs()
		{
			var dialog = new SaveFileDialog();
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				var file = System.IO.File.CreateText(dialog.FileName);
				file.Write(scintilla1.Text);
				file.Close();

				openFilesBar.Items.Remove(CurrentFile.TabButton);

				var newFile = PrepareFileTab(dialog.FileName, scintilla1.Text);
				SwitchToFile(newFile);

				return true;
			}
			return false;
		}

		private void sAVEASToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveAs();
		}

		private OpenFile PrepareFileTab(String Filename, String Text)
		{
			var tab = new FileTab();
			tab.Filename = System.IO.Path.GetFileName(Filename);
			tab.Font = scintilla1.Font;
			openFilesBar.Items.Add(tab);

			var openFile = new OpenFile
			{
				Filename = Filename,
				Text = Text,
				Changes = false,
				TabButton = tab
			};
			OpenFiles.Add(openFile);

			tab.Tag = openFile;
			
			tab.FilenameClick += (s, args) =>
			{
				SwitchToFile(openFile);
			};

			tab.FileClosing += (s, args) =>
				{
					var file = ((s as FileTab).Tag as OpenFile);

					if (file.Changes)
					{
						SwitchToFile(file);
						var r = MessageBox.Show(file.Filename + " has some unsaved changes. Save?", "Save", MessageBoxButtons.YesNoCancel);
						if (r == System.Windows.Forms.DialogResult.Cancel)
							return;
						if (r == System.Windows.Forms.DialogResult.Yes)
						{
							if (!Save()) return;
						}
					}

					OpenFiles.Remove(file);
					openFilesBar.Items.Remove(s as ToolStripItem);

					if (OpenFiles.Count > 0) SwitchToFile(OpenFiles[0]);
					else OpenNewFile();

					openFilesBar.Invalidate(true);
				};
			
			return openFile;
		}

		private bool Save()
		{
			if (CurrentFile.UnsavedNewFile)
				return SaveAs();
			else
			{
				var file = System.IO.File.CreateText(CurrentFile.Filename);
				CurrentFile.Text = scintilla1.Text;
				file.Write(scintilla1.Text);
				file.Close();
				CurrentFile.Changes = false;
				CurrentFile.TabButton.Changes = false;
				openFilesBar.Invalidate(true);
				return true;
			}
		}

		private void sAVEToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Save();
		}
		
		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (var file in OpenFiles)
			{
				if (file.Changes)
				{
					SwitchToFile(file);
					var r = MessageBox.Show(file.Filename + " has some unsaved changes. Save?", "Save", MessageBoxButtons.YesNoCancel);
					
					if (r == System.Windows.Forms.DialogResult.Cancel)
					{
						e.Cancel = true;
						return;
					}

					if (r == System.Windows.Forms.DialogResult.Yes)
					{
						if (!Save())
						{
							e.Cancel = true;
							return;
						}
					}
				}
			}

			Settings.OpenFiles = new List<String>(OpenFiles.Where(of => !of.UnsavedNewFile).Select(of => of.Filename));
			System.IO.File.WriteAllText("default-settings.txt", Newtonsoft.Json.JsonConvert.SerializeObject(Settings));
		}

		private void openFilesBar_MouseMove(object sender, MouseEventArgs e)
		{
			openFilesBar.Invalidate(true);
		}

		private void scintilla1_TextDeleted(object sender, TextModifiedEventArgs e)
		{
			if (!EnableChangeDetection) return;
			CurrentFile.Changes = true;
			CurrentFile.TabButton.Changes = true;
			openFilesBar.Invalidate(true);
		}

		private void scintilla1_TextInserted(object sender, TextModifiedEventArgs e)
		{
			if (!EnableChangeDetection) return;
			CurrentFile.Changes = true;
			CurrentFile.TabButton.Changes = true;
			openFilesBar.Invalidate(true);
		}

		private void nEWToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenNewFile();
		}

	}
}
