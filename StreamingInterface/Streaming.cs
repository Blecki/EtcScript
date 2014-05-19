using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StreamingInterface
{
	public partial class Streaming : UserControl
	{
		public Driver Driver;

		public void AppendText(WebBrowser Browser, String text)
		{
			Browser.Document.Write(text);
			Browser.Document.All[output.Document.All.Count - 1].ScrollIntoView(false);
		}

		public Streaming()
		{
			InitializeComponent();
		}
				
		public void LoadGame(String GameFile, Action<String> OnErrors)
		{
			Driver = new Driver(
				(String, Target) =>
				{
					switch (Target)
					{
						case StreamingInterface.Driver.WriteTarget.Main:
							AppendText(output, String);
							break;
						case StreamingInterface.Driver.WriteTarget.CommandList:
							AppendText(commandWindow, String);
							break;
					}
				},
				(Target) =>
				{
					switch (Target)
					{
						case StreamingInterface.Driver.WriteTarget.Main:
							output.Navigate("about:blank");
							break;
						case StreamingInterface.Driver.WriteTarget.CommandList:
							commandWindow.Navigate("about:blank");
							break;
					}
				});

			output.Navigate("about:blank");
			commandWindow.Navigate("about:blank");

			Driver.LoadGame(GameFile, OnErrors);

			this.Invalidate(true);
		}

		private void LinkHandler(object sender, WebBrowserNavigatingEventArgs e)
		{
			if (e.Url.OriginalString == "about:blank") return;
			e.Cancel = true;
			Driver.HandleLink(e.Url.LocalPath);
		}
	}
}
