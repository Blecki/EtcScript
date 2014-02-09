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
			var script = @"activity foo
	foreach x in [foo]
		bar x
";
			Console.WriteLine("Test script: " + script);
			var ops = EtcScriptLib.Compile.GetDefaultOperators();
			var declarations = EtcScriptLib.ParseToAst.Build(
				new EtcScriptLib.TokenStream(new EtcScriptLib.Compile.StringIterator(script), ops), ops,
				(s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });

		}
	}
}
