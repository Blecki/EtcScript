using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public class RuntimeScriptObject
	{
		public List<Object> Data;

		public RuntimeScriptObject(int Size)
		{
			Data = new List<object>();
			for (int i = 0; i < Size; ++i) Data.Add(null);
		}
	}
}
