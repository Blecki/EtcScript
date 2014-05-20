namespace EtcScriptEmu
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.scintilla1 = new ScintillaNET.Scintilla();
			this.menuBar = new System.Windows.Forms.ToolStrip();
			this.fileButton = new System.Windows.Forms.ToolStripDropDownButton();
			this.openButton = new System.Windows.Forms.ToolStripMenuItem();
			this.sAVEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sAVEASToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nEWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.setHostButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.hostLabel = new System.Windows.Forms.ToolStripLabel();
			this.compileButton = new System.Windows.Forms.ToolStripButton();
			this.runButton = new System.Windows.Forms.ToolStripButton();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.openFilesBar = new System.Windows.Forms.ToolStrip();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.defaultHosts = new System.Windows.Forms.ToolStripDropDownButton();
			((System.ComponentModel.ISupportInitialize)(this.scintilla1)).BeginInit();
			this.menuBar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// scintilla1
			// 
			this.scintilla1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scintilla1.Folding.UseCompactFolding = true;
			this.scintilla1.Font = new System.Drawing.Font("Envy Code R", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.scintilla1.Indentation.ShowGuides = true;
			this.scintilla1.Indentation.TabWidth = 4;
			this.scintilla1.IsBraceMatching = true;
			this.scintilla1.LineWrapping.Mode = ScintillaNET.LineWrappingMode.Word;
			this.scintilla1.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.Start;
			this.scintilla1.Location = new System.Drawing.Point(0, 0);
			this.scintilla1.Margins.Margin0.Width = 48;
			this.scintilla1.Margins.Margin2.Width = 16;
			this.scintilla1.Name = "scintilla1";
			this.scintilla1.Size = new System.Drawing.Size(560, 327);
			this.scintilla1.Styles.BraceBad.CharacterSet = ScintillaNET.CharacterSet.Ansi;
			this.scintilla1.Styles.BraceBad.FontName = "Envy Code R";
			this.scintilla1.Styles.BraceBad.Size = 9.75F;
			this.scintilla1.Styles.BraceLight.CharacterSet = ScintillaNET.CharacterSet.Ansi;
			this.scintilla1.Styles.BraceLight.FontName = "Envy Code R";
			this.scintilla1.Styles.BraceLight.ForeColor = System.Drawing.Color.Red;
			this.scintilla1.Styles.BraceLight.Size = 9.75F;
			this.scintilla1.Styles.CallTip.FontName = "Segoe UI\0\0\0";
			this.scintilla1.Styles.ControlChar.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.ControlChar.Size = 9F;
			this.scintilla1.Styles.Default.BackColor = System.Drawing.SystemColors.Window;
			this.scintilla1.Styles.Default.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.Default.Size = 9F;
			this.scintilla1.Styles.IndentGuide.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.IndentGuide.Size = 9F;
			this.scintilla1.Styles.LastPredefined.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.LastPredefined.Size = 9F;
			this.scintilla1.Styles.LineNumber.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.LineNumber.Size = 9F;
			this.scintilla1.Styles.Max.FontName = "Verdana\0\0\0\0";
			this.scintilla1.Styles.Max.Size = 9F;
			this.scintilla1.TabIndex = 0;
			this.scintilla1.StyleNeeded += new System.EventHandler<ScintillaNET.StyleNeededEventArgs>(this.scintilla1_StyleNeeded);
			this.scintilla1.TextDeleted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.scintilla1_TextDeleted);
			this.scintilla1.TextInserted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.scintilla1_TextInserted);
			// 
			// menuBar
			// 
			this.menuBar.Dock = System.Windows.Forms.DockStyle.None;
			this.menuBar.Font = new System.Drawing.Font("Envy Code R", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileButton,
            this.toolStripSeparator1,
            this.setHostButton,
            this.defaultHosts,
            this.toolStripSeparator2,
            this.hostLabel,
            this.compileButton,
            this.runButton});
			this.menuBar.Location = new System.Drawing.Point(3, 25);
			this.menuBar.Name = "menuBar";
			this.menuBar.Size = new System.Drawing.Size(302, 25);
			this.menuBar.TabIndex = 2;
			this.menuBar.Text = "toolStrip1";
			// 
			// fileButton
			// 
			this.fileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.fileButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openButton,
            this.sAVEToolStripMenuItem,
            this.sAVEASToolStripMenuItem,
            this.nEWToolStripMenuItem});
			this.fileButton.Image = ((System.Drawing.Image)(resources.GetObject("fileButton.Image")));
			this.fileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.fileButton.Name = "fileButton";
			this.fileButton.Size = new System.Drawing.Size(44, 22);
			this.fileButton.Text = "FILE";
			// 
			// openButton
			// 
			this.openButton.Name = "openButton";
			this.openButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openButton.Size = new System.Drawing.Size(195, 22);
			this.openButton.Text = "OPEN";
			this.openButton.Click += new System.EventHandler(this.oPENToolStripMenuItem1_Click);
			// 
			// sAVEToolStripMenuItem
			// 
			this.sAVEToolStripMenuItem.Name = "sAVEToolStripMenuItem";
			this.sAVEToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.sAVEToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.sAVEToolStripMenuItem.Text = "SAVE";
			this.sAVEToolStripMenuItem.Click += new System.EventHandler(this.sAVEToolStripMenuItem_Click);
			// 
			// sAVEASToolStripMenuItem
			// 
			this.sAVEASToolStripMenuItem.Name = "sAVEASToolStripMenuItem";
			this.sAVEASToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.sAVEASToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.sAVEASToolStripMenuItem.Text = "SAVE AS";
			this.sAVEASToolStripMenuItem.Click += new System.EventHandler(this.sAVEASToolStripMenuItem_Click);
			// 
			// nEWToolStripMenuItem
			// 
			this.nEWToolStripMenuItem.Name = "nEWToolStripMenuItem";
			this.nEWToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.nEWToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
			this.nEWToolStripMenuItem.Text = "NEW";
			this.nEWToolStripMenuItem.Click += new System.EventHandler(this.nEWToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// setHostButton
			// 
			this.setHostButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.setHostButton.Image = ((System.Drawing.Image)(resources.GetObject("setHostButton.Image")));
			this.setHostButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.setHostButton.Name = "setHostButton";
			this.setHostButton.Size = new System.Drawing.Size(35, 22);
			this.setHostButton.Text = "HOST";
			this.setHostButton.ToolTipText = "Select host assembly to use for compiling";
			this.setHostButton.Click += new System.EventHandler(this.sETHOSTToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// hostLabel
			// 
			this.hostLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.hostLabel.Name = "hostLabel";
			this.hostLabel.Size = new System.Drawing.Size(61, 22);
			this.hostLabel.Text = "[no host]";
			// 
			// compileButton
			// 
			this.compileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.compileButton.Image = ((System.Drawing.Image)(resources.GetObject("compileButton.Image")));
			this.compileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.compileButton.Name = "compileButton";
			this.compileButton.Size = new System.Drawing.Size(53, 22);
			this.compileButton.Text = "COMPILE";
			this.compileButton.ToolTipText = "Compile the current file";
			this.compileButton.Click += new System.EventHandler(this.cOMPILEToolStripMenuItem_Click);
			// 
			// runButton
			// 
			this.runButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.runButton.Image = ((System.Drawing.Image)(resources.GetObject("runButton.Image")));
			this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.runButton.Name = "runButton";
			this.runButton.Size = new System.Drawing.Size(29, 22);
			this.runButton.Text = "RUN";
			this.runButton.ToolTipText = "Run the current file";
			this.runButton.Click += new System.EventHandler(this.hOSTToolStripMenuItem_Click);
			// 
			// treeView1
			// 
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.Font = new System.Drawing.Font("Envy Code R", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Name = "treeView1";
			this.treeView1.Size = new System.Drawing.Size(117, 385);
			this.treeView1.TabIndex = 3;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.scintilla1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.textBox1);
			this.splitContainer1.Size = new System.Drawing.Size(560, 385);
			this.splitContainer1.SplitterDistance = 327;
			this.splitContainer1.TabIndex = 4;
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Font = new System.Drawing.Font("Envy Code R", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(560, 54);
			this.textBox1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.treeView1);
			this.splitContainer2.Size = new System.Drawing.Size(681, 385);
			this.splitContainer2.SplitterDistance = 560;
			this.splitContainer2.TabIndex = 5;
			// 
			// openFilesBar
			// 
			this.openFilesBar.Dock = System.Windows.Forms.DockStyle.None;
			this.openFilesBar.Font = new System.Drawing.Font("Envy Code R", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.openFilesBar.Location = new System.Drawing.Point(39, 0);
			this.openFilesBar.Name = "openFilesBar";
			this.openFilesBar.Size = new System.Drawing.Size(111, 25);
			this.openFilesBar.TabIndex = 6;
			this.openFilesBar.Text = "toolStrip2";
			this.openFilesBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.openFilesBar_MouseMove);
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer2);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(681, 385);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(681, 435);
			this.toolStripContainer1.TabIndex = 7;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuBar);
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.openFilesBar);
			// 
			// defaultHosts
			// 
			this.defaultHosts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.defaultHosts.Image = ((System.Drawing.Image)(resources.GetObject("defaultHosts.Image")));
			this.defaultHosts.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.defaultHosts.Name = "defaultHosts";
			this.defaultHosts.Size = new System.Drawing.Size(56, 22);
			this.defaultHosts.Text = "PRESET";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(681, 435);
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "Main";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.scintilla1)).EndInit();
			this.menuBar.ResumeLayout(false);
			this.menuBar.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ScintillaNET.Scintilla scintilla1;
		private System.Windows.Forms.ToolStrip menuBar;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.ToolStripDropDownButton fileButton;
		private System.Windows.Forms.ToolStripMenuItem openButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton setHostButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripLabel hostLabel;
		private System.Windows.Forms.ToolStripButton compileButton;
		private System.Windows.Forms.ToolStripButton runButton;
		private System.Windows.Forms.ToolStrip openFilesBar;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStripMenuItem sAVEToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem sAVEASToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem nEWToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton defaultHosts;
	}
}

