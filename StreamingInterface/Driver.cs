using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StreamingInterface
{
	public class Driver
	{
		public enum WriteTarget
		{
			Main,
			CommandList
		}

		private WriteTarget CurrentTarget = WriteTarget.Main;
		internal EtcScriptLib.Environment ScriptEnvironment;
		EtcScriptLib.VirtualMachine.ExecutionContext ActiveContext;

		Action<String, WriteTarget> Output;
		Action<WriteTarget> Clear;

		public Driver(Action<String, WriteTarget> Output, Action<WriteTarget> Clear)
		{
			this.Output = Output;
			this.Clear = Clear;
		}

		public void DisplayError(String ErrorMessage)
		{
			Clear(WriteTarget.Main);
			Output(ErrorMessage, WriteTarget.Main);
		}

		public void LoadGame(String GameFile, Action<String> OnErrors)
		{
			PrepareEnvironment();

			if (!Compile(GameFile, (s) => { DisplayError(s); OnErrors(s); }))
			{
				RunScript("macro _ { consider [startup]; }", new List<Object>());
			}
		}

		public void PrepareEnvironment()
		{
			ScriptEnvironment = new EtcScriptLib.Environment();

			ScriptEnvironment.AddSystemMacro("target main", (context, arguments) => { CurrentTarget = WriteTarget.Main; return null; });
			ScriptEnvironment.AddSystemMacro("target bottom", (context, arguments) => { CurrentTarget = WriteTarget.CommandList; return null; });
			ScriptEnvironment.AddSystemMacro("clear", (context, arguments) => { Clear(CurrentTarget); return null; });
			ScriptEnvironment.AddSystemMacro("write (s:string)", (context, arguments) => { Output(arguments[0].ToString(), CurrentTarget); return null; });
		}

		public bool Compile(String GameFile, Action<String> OnError)
		{
			var encounteredError = false;
			var script = "include \"" + GameFile.Replace("\\", "\\\\") + "\"";

			ScriptEnvironment.Build(script, (str) =>
			{
				encounteredError = true;
				OnError(str);
				return EtcScriptLib.ErrorStrategy.Abort;
			});

			return encounteredError;
		}

		private void RunScript(String Script, List<Object> Arguments)
		{
			var declaration = ScriptEnvironment.CompileDeclaration(Script);
			var invokable = declaration.MakeInvokableFunction();
			Arguments.Insert(0, invokable);
			ActiveContext = ScriptEnvironment.CreateExecutionContext(EtcScriptLib.VirtualMachine.ExecutionLocation.Empty);
			invokable.Invoke(ActiveContext, Arguments);
			EtcScriptLib.VirtualMachine.VirtualMachine.ExecuteUntilFinished(ActiveContext);
		}

		public void HandleLink(String Link)
		{
			RunScript("macro _ { " + Link + "; }", new List<Object>());
		}
	}
}
