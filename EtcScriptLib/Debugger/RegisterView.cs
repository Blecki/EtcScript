using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EtcScriptLib.Debugger
{
	public partial class RegisterView : UserControl
	{
		public VirtualMachine.ExecutionContext Context;

		public RegisterView()
		{
			InitializeComponent();
		}

		private void RegisterView_Paint(object sender, PaintEventArgs e)
		{
			Font font = new Font(FontFamily.GenericMonospace, 12);
			e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);

			if (this.DesignMode || Context == null)
			{
				e.Graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
				return;
			}

			Size baseFontSize = TextRenderer.MeasureText("R", font);

			e.Graphics.DrawString("R: " + (Context.R == null ? "null" : Context.R.ToString()), font, Brushes.Gray, 2, 0);
			e.Graphics.DrawString("F: " + Context.F.ToString(), font, Brushes.Gray, 2, baseFontSize.Height + 2);
		}
	}
}
