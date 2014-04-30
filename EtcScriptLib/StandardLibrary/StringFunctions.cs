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
			Environment.AddScriptMacro(@"MACRO CONVERT (S:COMPLEXSTRING) TO STRING : STRING { RETURN [INVOKE [S]]:STRING; }");
			Environment.AddScriptMacro(@"MACRO CONVERT (S:STRING) TO COMPLEXSTRING : COMPLEXSTRING { RETURN $""[S]""; }");
			Environment.AddSystemMacro("CONVERT (N:NUMBER) TO STRING : STRING", (c, l) => { return l[0].ToString(); });

			Environment.AddSystemMacro(
				"length of (s:string) : number",
				(context, arguments) =>
				{
					return (float)(arguments[0] as String).Length;
				});

			Environment.AddSystemMacro(
				"GET AT (N:NUMBER) FROM (S:STRING) : CHAR",
				(context, arguments) =>
				{
					return (arguments[1] as String)[Convert.ToInt32(arguments[0])];
				});

			Environment.AddSystemMacro(
				"CONVERT (C:CHAR) TO STRING : STRING",
				(context, arguments) =>
				{
					return new String((arguments[0] as char?).Value, 1);
				});
		}
	}
}
