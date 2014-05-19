using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamingInterface
{
	public class ExternalHost : EtcScriptLib.IExternalHost
	{
		public bool SupportsHosting() {	return true; }
		public string Name() { return "Classic Stream"; }

		public System.Windows.Forms.Control Host(
			string HostFilename, 
			string Filename, 
			Action<string> Errors, 
			Action<EtcScriptLib.Environment> FinishedCompiling)
		{
			var realFilename = System.IO.Path.GetFullPath(Filename);
			var s = new Streaming();
			s.LoadGame(realFilename, Errors);
			FinishedCompiling(s.Driver.ScriptEnvironment);
			return s;
		}

		public System.Drawing.Size PreferredSize()
		{
			return new System.Drawing.Size(800, 600);
		}

		public void Compile(
			string HostFilename, 
			string Filename, 
			Action<string> Errors, 
			Action<EtcScriptLib.Environment> FinishedCompiling)
		{
			var realFilename = System.IO.Path.GetFullPath(Filename);
			var d = new Driver((s,t) => {}, (t) => {});
			d.PrepareEnvironment();
			d.Compile(realFilename, Errors);
			FinishedCompiling(d.ScriptEnvironment);
		}
	}
}
