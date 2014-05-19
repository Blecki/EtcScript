using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace EtcScriptLib
{
	public interface IExternalLibrary
	{
		void BindLibrary(Environment Environment);
	}

	public class ExternalLibrary
	{
		public static Action<String> Debug;

		public static void LoadAssembly(
			String Filename, 
			Environment Environment)
		{
			var assembly = Assembly.LoadFile(Filename);
			foreach (var possibleLibrary in assembly.GetExportedTypes())
			{
				var interfaces = possibleLibrary.GetInterfaces();
				var isLibrary = interfaces.Count(i => i == typeof(IExternalLibrary)) > 0;
				if (isLibrary)
				{
					if (Debug != null) Debug("Loading library " + possibleLibrary.Name);
					var instance = Activator.CreateInstance(possibleLibrary);
					var externalLibrary = instance as IExternalLibrary;
					if (externalLibrary != null)
						externalLibrary.BindLibrary(Environment);
					else
						if (Debug != null) Debug("Library creation failed.");
				}
			}
		}
	}
}
