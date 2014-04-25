using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void MathFunctions(Environment Environment)
		{
			Environment.AddSystemMacro("floor (n:number) : number",
				(c, a) => { return (float)(Math.Floor(Convert.ToSingle(a[0]))); });
			Environment.AddSystemMacro("ceil (n:number) : number",
				(c, a) => { return (float)(Math.Ceiling(Convert.ToSingle(a[0]))); });
			Environment.AddSystemMacro("abs (n:number) : number",
				(c, a) => { return (float)(Math.Abs(Convert.ToSingle(a[0]))); });
		}
	}
}
