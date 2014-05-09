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
			this.scintilla1 = new ScintillaNET.Scintilla();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.oPENToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.cOMPILEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.scintilla1)).BeginInit();
			this.menuStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
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
			this.scintilla1.Size = new System.Drawing.Size(560, 331);
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
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.cOMPILEToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(681, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oPENToolStripMenuItem1});
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.openToolStripMenuItem.Text = "FILE";
			// 
			// oPENToolStripMenuItem1
			// 
			this.oPENToolStripMenuItem1.Name = "oPENToolStripMenuItem1";
			this.oPENToolStripMenuItem1.Size = new System.Drawing.Size(105, 22);
			this.oPENToolStripMenuItem1.Text = "OPEN";
			this.oPENToolStripMenuItem1.Click += new System.EventHandler(this.oPENToolStripMenuItem1_Click);
			// 
			// cOMPILEToolStripMenuItem
			// 
			this.cOMPILEToolStripMenuItem.Name = "cOMPILEToolStripMenuItem";
			this.cOMPILEToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
			this.cOMPILEToolStripMenuItem.Text = "COMPILE";
			this.cOMPILEToolStripMenuItem.Click += new System.EventHandler(this.cOMPILEToolStripMenuItem_Click);
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(681, 25);
			this.toolStrip1.TabIndex = 2;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// treeView1
			// 
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.Location = new System.Drawing.Point(0, 0);
			this.treeView1.Name = "treeView1";
			this.treeView1.Size = new System.Drawing.Size(117, 386);
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
			this.splitContainer1.Size = new System.Drawing.Size(560, 386);
			this.splitContainer1.SplitterDistance = 331;
			this.splitContainer1.TabIndex = 4;
			// 
			// textBox1
			// 
			this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBox1.Location = new System.Drawing.Point(0, 0);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(560, 51);
			this.textBox1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 49);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.treeView1);
			this.splitContainer2.Size = new System.Drawing.Size(681, 386);
			this.splitContainer2.SplitterDistance = 560;
			this.splitContainer2.TabIndex = 5;
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(681, 435);
			this.Controls.Add(this.splitContainer2);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Main";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.scintilla1)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ScintillaNET.Scintilla scintilla1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem oPENToolStripMenuItem1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.ToolStripMenuItem cOMPILEToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.SplitContainer splitContainer2;
	}
}

