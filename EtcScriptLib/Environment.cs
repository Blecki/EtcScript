using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class Environment
	{
		public Dictionary<String, Action<Declaration>> DeclarationHandlers = 
			new Dictionary<string, Action<Declaration>>();
		public ParseContext Context = Compile.GetDefaultOperators();
		public VirtualMachine.ScriptObject GlobalScope = new VirtualMachine.ScriptObject();

		private List<Declaration> Functions = new List<Declaration>();

		public List<Declaration> Build(String script, Func<String, ErrorStrategy> OnError)
		{
			return Compile.Build(script, Context, OnError);
		}

		public Declaration CompileString(String script)
		{
			var _script = "macro _@\n\t" + script;
			var stream = new TokenStream(new Compile.StringIterator(_script), Context);
			var declaration = Parse.ParseDeclaration(stream, Context);
			return declaration;
		}

		public void AddSystemMacro(String Header, Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			var decl = Declaration.Parse(Header);
			decl.Body.CacheSystemImplementation(Implementation);
			Context.ActiveScope.Macros.Add(decl);
		}

		public void AddControl(Control Control)
		{
			Context.AddControl(Control);
		}

		public static Environment CreateStandardEnvironment()
		{
			var environment = new EtcScriptLib.Environment();
			return environment;
		}
	}
}
