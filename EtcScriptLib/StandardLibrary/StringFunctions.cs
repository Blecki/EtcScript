using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void StringFunctions(ParseContext Context)
		{
			Context.ActiveScope.Macros.Add(Environment.PrepareSystemMacro(
				"length of (s:string) : number",
				(context, arguments) =>
				{
					return (int)(arguments[0] as String).Length;
				}));

			Context.ActiveScope.Macros.Add(Environment.PrepareSystemMacro(
				"(s:string) at (n:number) : number",
				(context, arguments) =>
				{
					return (int)(arguments[0] as String)[(arguments[1] as int?).Value];
				}));
		}
	}
}
