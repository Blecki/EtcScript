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
			var x = new EtcScriptTests.Compilation();
			x.interpret_semicolon_as_newline_with_equal_tabs();

//            var script = @"perform temporary_state state
//	foreach step in [path.QuantizePath 0.25]
//		do nothing
//";
//            Console.WriteLine("Test script: " + script);
//            var ops = EtcScriptLib.Compile.GetDefaultOperators();
//            var declarations = EtcScriptLib.ParseToAst.Build(
//                new EtcScriptLib.TokenStream(new EtcScriptLib.Compile.StringIterator(script), ops), ops,
//                (s) => { Console.WriteLine(s); return EtcScriptLib.ErrorStrategy.Continue; });

		}
	}
}
