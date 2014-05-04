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
	public partial class StackView : UserControl
	{
		public VirtualMachine.ExecutionContext Context;
		public VScrollBar Scrollbar;

		public StackView()
		{
			InitializeComponent();
		}

		private void StackView_Paint(object sender, PaintEventArgs e)
		{
			Font font = new Font(FontFamily.GenericMonospace, 12);
			e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);

			if (this.DesignMode || Context == null)
			{
				e.Graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
				return;
			}

			Size baseFontSize = TextRenderer.MeasureText("000", font);
			int lineHeight = baseFontSize.Height + 2;
			int visibleLines = this.ClientRectangle.Height / lineHeight;

			var dark = true;

			var top = Scrollbar.Value;
			
			for (int y = 0; y < this.Height; y += lineHeight)
			{
				if (top >= Context.Stack.Count) break;

				string address = top.ToString();
				if (address.Length < 3) address += new String(' ', 3 - address.Length);
				address += ": ";

				var addressFontSize = TextRenderer.MeasureText(address, font);
				Brush foreground = Brushes.Black;

				if (dark)
					e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 230, 230, 230)), new Rectangle(0, y, this.Width, addressFontSize.Height + 2));
				dark = !dark;

				e.Graphics.DrawString(address, font, Brushes.Gray, 2, y);

				var disassembly = Context.Stack[top] == null ? "null" : Context.Stack[top].ToString();
				e.Graphics.DrawString(disassembly, font, foreground, 2 + addressFontSize.Width + 3, y);

				++top;
			}
		}
	}
}
