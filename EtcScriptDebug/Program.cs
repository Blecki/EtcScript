using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptDebug
{
	class Program
	{
		static void Main(string[] args)
		{
			//var traceFile = System.IO.File.Open("trace.txt", System.IO.FileMode.Create);
			//var writer = new System.IO.StreamWriter(traceFile);
			//EtcScriptLib.VirtualMachine.VirtualMachine.DetailedTracing = true;
			//EtcScriptLib.VirtualMachine.VirtualMachine.WriteTraceLine += (s) =>
			//    {
			//        writer.WriteLine(s);
			//        writer.Flush();
			//        traceFile.Flush();
			//    };

			try
			{
				var x = new EtcScriptTests.Types();
				x.system_setter();
			}
			catch (Exception e)
			{
				//writer.WriteLine("ERROR: " + e.Message + e.StackTrace);
			}

			//writer.Flush();
			//writer.Close();

			//traceFile.Close();

		}
	}
}
