using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public interface IExternalHost
	{
		bool SupportsHosting();
		String Name();

		System.Windows.Forms.Control Host(
			String HostFilename, 
			String Filename,
			Action<String> Errors,
			Action<Environment> FinishedCompiling);

		System.Drawing.Size PreferredSize();

		void Compile(
			String HostFilename,
			String Filename,
			Action<String> Errors,
			Action<Environment> FinishedCompiling);
	}
}
