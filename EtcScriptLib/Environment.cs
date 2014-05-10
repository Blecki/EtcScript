using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class SystemVariable : Variable
	{
		public VirtualMachine.InvokeableFunction Implementation;

		public SystemVariable(String Name, Func<VirtualMachine.ExecutionContext, Object> Implementation, String Typename)
		{
			this.Implementation = new VirtualMachine.NativeFunction("[sys-variable] " + Name, (c, l) => Implementation(c));
			this.StorageMethod = VariableStorageMethod.System;
			this.DeclaredTypeName = Typename;
			this.Name = Name;
		}
	}

	public class CompileTimeConstant : Variable
	{
		public Object Value;

		public CompileTimeConstant(String Name, Object Value, String Typename)
		{
			this.Value = Value;
			this.StorageMethod = VariableStorageMethod.Constant;
			this.DeclaredTypeName = Typename;
			this.Name = Name;
		}
	}

	public class LoadedFile
	{
		public String Data;
		public String Directory;
	}

	public class Environment
	{
		public ParseContext Context;
		private Dictionary<int, Object> StaticVariableStorage = new Dictionary<int, object>();
		private int StaticVariableCount = 0;
		public Include FileLoader;

		public Environment()
		{
			Context = new ParseContext();
			Compile.GetDefaultParseContext(this);
			FileLoader = new Include();
		}

		public Debugger.Debugger OpenDebuggerWindow(VirtualMachine.ExecutionContext Context)
		{
			var window = new Debugger.Debugger(Context);
			window.Show();
			return window;
		}

		public VirtualMachine.ExecutionContext CreateExecutionContext(VirtualMachine.CodeContext EntryPoint)
		{
			return new VirtualMachine.ExecutionContext(StaticVariableStorage, Context.TopScope.Types, EntryPoint);
		}

		public List<Declaration> Build(String script, Func<String, ErrorStrategy> OnError, bool DelayEmission = false)
		{
			Context.FileLoader = FileLoader;
			var testFunctions = Compile.Build(script, Context, StaticVariableCount, OnError, DelayEmission);
			StaticVariableCount += Context.StaticVariableCount;

			try
			{
				if (!DelayEmission) RunInitializer();
			}
			catch (Exception e)
			{
				OnError(e.Message);
			}
			return testFunctions;
		}

		public void RunInitializer()
		{
			if (Context.InitializationFunction == null) return;

			var initializerContext = CreateExecutionContext(VirtualMachine.CodeContext.Empty);
			var invokeable = Context.InitializationFunction.MakeInvokableFunction();
			invokeable.Invoke(initializerContext, new List<Object>(new Object[] { invokeable }));
			VirtualMachine.VirtualMachine.ExecuteUntilFinished(initializerContext);
		}

		public Declaration CompileDeclaration(String script)
		{
			Context.ID = 0;

			var stream = new TokenStream(new StringIterator(script), Context);
			var declaration = Parse.ParseMacroDeclaration(stream, Context);

			declaration.ResolveTypes(Context.ActiveScope);
			declaration.Transform(0);

			var instructionStream = new VirtualMachine.InstructionList();
			declaration.Emit(instructionStream);
			declaration.ResolveCallPoints();
			instructionStream.StringTable.Compress();

			if (Compile.Debug)
			{
				Compile.EmitDebugDump(declaration);
				Compile.DebugWrite("\n");

				Compile.DebugWrite("\nCOMPILED CODE:\n");
				VirtualMachine.Debug.DumpOpcode(instructionStream.Data, Compile.DebugWrite, 1);
				Compile.DebugWrite(" STRING TABLE:\n");
				for (int i = 0; i < instructionStream.StringTable.PartTable.Length; ++i)
					Compile.DebugWrite(" " + i.ToString() + ": " + instructionStream.StringTable.PartTable[i] + "\n");
				Compile.DebugWrite(" STRING DATA:\n");
				Compile.DebugWrite("  " + instructionStream.StringTable.StringData + "\n");
			}

			return declaration;
		}
			
		public Declaration CompileString(String script)
		{
			return CompileDeclaration("macro _ { " + script + " }");
		}

		public void AddSystemMacro(String Header, Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			Context.ActiveScope.Macros.Add(PrepareSystemMacro(Header, Implementation));
		}

		public void AddScriptMacro(String Script)
		{
			var stream = new TokenStream(new StringIterator(Script), Context);
			var declaration = Parse.ParseMacroDeclaration(stream, Context);
			declaration.OwnerContextID = Context.ID;
			Context.PendingEmission.Add(declaration);
		}

		public void AddSystemType(String Name)
		{
			Context.ActiveScope.Types.Add(new Type(null) { Origin = TypeOrigin.System, Name = Name.ToUpper() });
		}

		public static Declaration PrepareSystemMacro(String Header,
			Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			var decl = Declaration.Parse(Header);
			var argumentCount = decl.Terms.Count(t => t.Type == DeclarationTermType.Term);
			decl.Body.CacheSystemImplementation(argumentCount, decl.DescriptiveHeader, Implementation);
			decl.Type = DeclarationType.System;
			return decl;
		}

		public void AddSystemVariable(String Name, String ResultTypeName, Func<VirtualMachine.ExecutionContext, Object> Implementation)
		{
			var variable = new SystemVariable(Name.ToUpper(), Implementation, ResultTypeName.ToUpper());
			Context.ActiveScope.Variables.Add(variable);
		}

		public void AddCompileTimeConstant(String Name, String ResultTypeName, Object Value)
		{
			var variable = new CompileTimeConstant(Name.ToUpper(), Value, ResultTypeName.ToUpper());
			Context.TopScope.Variables.Add(variable);
		}

		public void AddControl(Control Control)
		{
			Context.AddControl(Control);
		}
	}
}
