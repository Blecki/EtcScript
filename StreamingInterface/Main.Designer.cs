namespace StreamingInterface
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
			this.scriptStream = new StreamingInterface.Streaming();
			this.SuspendLayout();
			// 
			// scriptStream
			// 
			this.scriptStream.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scriptStream.Location = new System.Drawing.Point(0, 0);
			this.scriptStream.Name = "scriptStream";
			this.scriptStream.Size = new System.Drawing.Size(284, 262);
			this.scriptStream.TabIndex = 0;
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.scriptStream);
			this.Name = "Main";
			this.Text = "EtcScript";
			this.ResumeLayout(false);

		}

		#endregion

		private Streaming scriptStream;



	}
}

