namespace EtcScriptLib.Debugger
{
	partial class Debugger
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
			this.stepButton = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.stackScrollBar = new System.Windows.Forms.VScrollBar();
			this.button1 = new System.Windows.Forms.Button();
			this.disassembledView = new EtcScriptLib.Debugger.AssemblyView();
			this.stackView = new EtcScriptLib.Debugger.StackView();
			this.registerView1 = new EtcScriptLib.Debugger.RegisterView();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// stepButton
			// 
			this.stepButton.Location = new System.Drawing.Point(3, 3);
			this.stepButton.Name = "stepButton";
			this.stepButton.Size = new System.Drawing.Size(75, 23);
			this.stepButton.TabIndex = 1;
			this.stepButton.Text = "STEP";
			this.stepButton.UseVisualStyleBackColor = true;
			this.stepButton.Click += new System.EventHandler(this.stepButton_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.stackView);
			this.panel1.Controls.Add(this.stackScrollBar);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel1.Location = new System.Drawing.Point(467, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(229, 524);
			this.panel1.TabIndex = 2;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.button1);
			this.panel2.Controls.Add(this.registerView1);
			this.panel2.Controls.Add(this.stepButton);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(229, 80);
			this.panel2.TabIndex = 0;
			// 
			// stackScrollBar
			// 
			this.stackScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.stackScrollBar.Location = new System.Drawing.Point(212, 80);
			this.stackScrollBar.Name = "stackScrollBar";
			this.stackScrollBar.Size = new System.Drawing.Size(17, 444);
			this.stackScrollBar.TabIndex = 2;
			this.stackScrollBar.ValueChanged += new System.EventHandler(this.stackScrollBar_ValueChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(84, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "RUN";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// disassembledView
			// 
			this.disassembledView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.disassembledView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.disassembledView.Location = new System.Drawing.Point(0, 0);
			this.disassembledView.Name = "disassembledView";
			this.disassembledView.Size = new System.Drawing.Size(467, 524);
			this.disassembledView.TabIndex = 0;
			// 
			// stackView
			// 
			this.stackView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.stackView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stackView.Location = new System.Drawing.Point(0, 80);
			this.stackView.Name = "stackView";
			this.stackView.Size = new System.Drawing.Size(212, 444);
			this.stackView.TabIndex = 1;
			// 
			// registerView1
			// 
			this.registerView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.registerView1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.registerView1.Location = new System.Drawing.Point(0, 32);
			this.registerView1.Name = "registerView1";
			this.registerView1.Size = new System.Drawing.Size(229, 48);
			this.registerView1.TabIndex = 2;
			// 
			// Debugger
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(696, 524);
			this.Controls.Add(this.disassembledView);
			this.Controls.Add(this.panel1);
			this.Name = "Debugger";
			this.Text = "Debugger";
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private AssemblyView disassembledView;
		private System.Windows.Forms.Button stepButton;
		private System.Windows.Forms.Panel panel1;
		private StackView stackView;
		private System.Windows.Forms.VScrollBar stackScrollBar;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button button1;
		private RegisterView registerView1;
	}
}