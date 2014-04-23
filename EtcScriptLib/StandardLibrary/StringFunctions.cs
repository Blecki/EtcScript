using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void StringFunctions(Environment Environment)
		{
			Environment.AddScriptMacro(@"MACRO CONVERT (S:COMPLEXSTRING) TO STRING : STRING { RETURN (:[S]):STRING; }");
			Environment.AddScriptMacro(@"MACRO CONVERT (S:STRING) TO COMPLEXSTRING : COMPLEXSTRING { RETURN @""[S]""; }");

			Environment.AddSystemMacro(
				"length of (s:string) : number",
				(context, arguments) =>
				{
					return (int)(arguments[0] as String).Length;
				});

			Environment.AddSystemMacro(
				"(s:string) at (n:number) : number",
				(context, arguments) =>
				{
					return (int)(arguments[0] as String)[(arguments[1] as int?).Value];
				});
		}
	}
}
