using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class Include
	{
		private List<String> FileList = new List<string>();

		public virtual LoadedFile LoadFile(String Filename, LoadedFile LoadedBy)
		{
			var relativeFilename = LoadedBy == null ? Filename : System.IO.Path.Combine(LoadedBy.Directory, Filename);
			var absolutePath = System.IO.Path.GetFullPath(relativeFilename);

			if (FileList.Contains(absolutePath))
				return new LoadedFile { Data = "" };
			FileList.Add(absolutePath);

			return new LoadedFile
			{
				Directory = System.IO.Path.GetDirectoryName(absolutePath),
				Data = System.IO.File.ReadAllText(absolutePath)
			};
		}
	}
}
