namespace StreamingInterface
{
	partial class Streaming
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.topSplit = new System.Windows.Forms.SplitContainer();
			this.output = new System.Windows.Forms.WebBrowser();
			this.commandWindow = new System.Windows.Forms.WebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.topSplit)).BeginInit();
			this.topSplit.Panel1.SuspendLayout();
			this.topSplit.Panel2.SuspendLayout();
			this.topSplit.SuspendLayout();
			this.SuspendLayout();
			// 
			// topSplit
			// 
			this.topSplit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topSplit.Location = new System.Drawing.Point(0, 0);
			this.topSplit.Name = "topSplit";
			this.topSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// topSplit.Panel1
			// 
			this.topSplit.Panel1.Controls.Add(this.output);
			// 
			// topSplit.Panel2
			// 
			this.topSplit.Panel2.Controls.Add(this.commandWindow);
			this.topSplit.Size = new System.Drawing.Size(478, 471);
			this.topSplit.SplitterDistance = 369;
			this.topSplit.TabIndex = 0;
			// 
			// output
			// 
			this.output.Dock = System.Windows.Forms.DockStyle.Fill;
			this.output.Location = new System.Drawing.Point(0, 0);
			this.output.MinimumSize = new System.Drawing.Size(20, 20);
			this.output.Name = "output";
			this.output.Size = new System.Drawing.Size(478, 369);
			this.output.TabIndex = 0;
			this.output.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.LinkHandler);
			// 
			// commandWindow
			// 
			this.commandWindow.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commandWindow.Location = new System.Drawing.Point(0, 0);
			this.commandWindow.MinimumSize = new System.Drawing.Size(20, 20);
			this.commandWindow.Name = "commandWindow";
			this.commandWindow.Size = new System.Drawing.Size(478, 98);
			this.commandWindow.TabIndex = 0;
			this.commandWindow.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.LinkHandler);
			// 
			// Streaming
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.topSplit);
			this.Name = "Streaming";
			this.Size = new System.Drawing.Size(478, 471);
			this.topSplit.Panel1.ResumeLayout(false);
			this.topSplit.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.topSplit)).EndInit();
			this.topSplit.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer topSplit;
		private System.Windows.Forms.WebBrowser output;
		private System.Windows.Forms.WebBrowser commandWindow;
	}
}
