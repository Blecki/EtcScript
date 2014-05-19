using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EtcScriptEmu
{
	public partial class FileTab : ToolStripItem
	{
		private String _Filename;
		private bool _Changes;

		public String Filename { get { return _Filename; } set { _Filename = value; Invalidate(); } }
		public bool Changes { get { return _Changes; } set { _Changes = value; Invalidate(); } }

		public event EventHandler FileClosing;
		public event EventHandler FilenameClick;

		private bool _mouseHover = false;
		private bool _overClose = false;
		private bool _currentTab = false;
		public bool CurrentTab { get { return _currentTab; } set { _currentTab = value; Invalidate(); } }

		public FileTab()
		{
			InitializeComponent();
		}

		private void FileTab_Paint(object sender, PaintEventArgs e)
		{
			if (_currentTab)
				e.Graphics.FillRectangle(new System.Drawing.Drawing2D.LinearGradientBrush(
					new Point(0, ContentRectangle.Y),
					new Point(0, ContentRectangle.Height + ContentRectangle.Y),
					BackColor,
					Color.Orange), this.ContentRectangle);
			//else
			//	e.Graphics.FillRectangle(new SolidBrush(BackColor), this.ContentRectangle);
			
			if (this.DesignMode)
			{
				e.Graphics.FillRectangle(Brushes.Black, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
				return;
			}

			var localMouse = Control.MousePosition;
			localMouse = this.Owner.PointToClient(localMouse);
			//localMouse.X -= this.Owner.DisplayRectangle.X;
			//localMouse.Y -= this.Owner.DisplayRectangle.Y;

			_mouseHover = Bounds.Contains(localMouse);
			_overClose = (localMouse.X - Bounds.X) >= (Bounds.Width - 14);

			if (_mouseHover)
			{
				if (_overClose)
				{
					var hiliteRectangle = new Rectangle();
					hiliteRectangle.X = ContentRectangle.X + (ContentRectangle.Width - 14);
					hiliteRectangle.Y = ContentRectangle.Y - 2;
					hiliteRectangle.Width = 14;
					hiliteRectangle.Height = ContentRectangle.Height + 3;
					e.Graphics.FillRectangle(Brushes.LightBlue, hiliteRectangle);
					e.Graphics.DrawRectangle(Pens.CornflowerBlue, hiliteRectangle);
				}
				else if (!_currentTab)
				{
					var hiliteRectangle = new Rectangle();
					hiliteRectangle.X = ContentRectangle.X;
					hiliteRectangle.Y = ContentRectangle.Y - 2;
					hiliteRectangle.Width = ContentRectangle.Width - 14;
					hiliteRectangle.Height = ContentRectangle.Height + 3;
					e.Graphics.FillRectangle(Brushes.LightBlue, hiliteRectangle);
					e.Graphics.DrawRectangle(Pens.CornflowerBlue, hiliteRectangle);
				}
			}


			e.Graphics.DrawString(_Changes ? _Filename + "*" : _Filename, Font, Brushes.Black, 2, 2);
			e.Graphics.DrawString("X", Font, Brushes.Red, this.Width - 14, 2);

			
		}

		private void Handle_Click(object sender, MouseEventArgs e)
		{
			if (e.X >= (this.Width - 14))
				FileClosing(this, null);
			else
				FilenameClick(this, null);
		}

		private void Handle_MouseEnter(object sender, EventArgs e)
		{
			Invalidate();
		}

		private void Handle_MouseLeave(object sender, EventArgs e)
		{
			Invalidate();
		}

		private void Handle_MouseMove(object sender, MouseEventArgs e)
		{
			Invalidate();
		}

		private void Handle_Mouse(object sender, MouseEventArgs e)
		{
			if (e.X > (this.Width - 14)) _overClose = true;
			else _overClose = false;
		}

		public override Size GetPreferredSize(Size constrainingSize)
		{
			var size = TextRenderer.MeasureText(_Changes ? _Filename + "*" : _Filename, Font);
			size.Width += 16;

			return size;
		}
	}
}
