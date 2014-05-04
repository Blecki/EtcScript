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
	public partial class AssemblyView : UserControl
	{
		public VirtualMachine.ExecutionContext Context;

		public AssemblyView()
		{
			InitializeComponent();
		}

		private void AssemblyView_Paint(object sender, PaintEventArgs e)
		{
			Font font = new Font(FontFamily.GenericMonospace, 12);
			e.Graphics.FillRectangle(Brushes.White, this.ClientRectangle);

			if (this.DesignMode || Context == null)
			{
				e.Graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
				return;
			}

			if (Context.CurrentInstruction.Code.Count == 0)
			{
				e.Graphics.DrawString("NO CODE TO DISPLAY", font, Brushes.Black, 0, 0);
				return;
			}

			Size baseFontSize = TextRenderer.MeasureText("000000", font);
			int lineHeight = baseFontSize.Height + 2;
			int visibleLines = this.ClientRectangle.Height / lineHeight;

			var iterator = Context.CurrentInstruction.Code.Data.GetIterator();
			iterator._place = Context.CurrentInstruction.InstructionPointer - (visibleLines / 2);
			if (iterator._place < 0) iterator._place = 0;

			var dark = true;

			for (int y = 0; y < this.Height; y += lineHeight)
			{
				if (iterator.AtEnd()) break;

				string address = iterator._place.ToString();
				if (address.Length < 5) address += new String(' ', 5 - address.Length);
				address += ": ";

				var addressFontSize = TextRenderer.MeasureText(address, font);
				Brush foreground = Brushes.Gray;

				if (dark)
					e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 230, 230, 230)), new Rectangle(0, y, this.Width, addressFontSize.Height + 2));
				dark = !dark;

				if (iterator._place == Context.CurrentInstruction.InstructionPointer)
				{
					e.Graphics.FillRectangle(Brushes.Yellow, new Rectangle(0, y + 2, this.Width, addressFontSize.Height - 2));
					foreground = Brushes.Black;
				}

				e.Graphics.DrawString(address, font, Brushes.Gray, 2, y);

				var disassembly = VirtualMachine.Debug.GetOpcodeString(iterator);
				e.Graphics.DrawString(disassembly, font, foreground, 2 + addressFontSize.Width + 1, y);

				iterator.Advance();
			}
		}

		private void AssemblyView_Resize(object sender, EventArgs e)
		{
			this.Invalidate();
		}
	}
}
