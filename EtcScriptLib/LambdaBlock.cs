using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class LambdaBlock
	{
		internal VirtualMachine.InstructionList Instructions;
		internal Ast.Node Body;
		private VirtualMachine.InvokeableFunction CachedLambda;

		public LambdaBlock(Ast.Node Body)
		{
			this.Body = Body;
		}

		public void EmitInstructions(ParseScope DeclarationScope)
		{
			if (Instructions != null) throw new InvalidOperationException("Instructions should not be emitted twice");

			Body.Transform(DeclarationScope);

			Instructions = new VirtualMachine.InstructionList();
			Instructions.AddInstructions("MARK_STACK R", "SET_VARIABLE R STRING", Instructions.AddString("@stack-size"));
			Body.Emit(Instructions, Ast.OperationDestination.Discard);
			Instructions.AddInstructions(
				"LOOKUP STRING PUSH", Instructions.AddString("@stack-size"),
				"RESTORE_STACK POP",
				"CONTINUE POP");
		}

		public void CacheSystemImplementation(Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			CachedLambda = new VirtualMachine.NativeFunction("SYS-LAMBDA", Implementation);
		}

		public VirtualMachine.InvokeableFunction GetInvokable(
			ParseScope DeclarationScope,
			List<DeclarationTerm> Terms, 
			VirtualMachine.ScriptObject CapturedScope = null)
		{
			if (CachedLambda == null)
			{
				if (Instructions == null)
					EmitInstructions(DeclarationScope);

				CachedLambda = VirtualMachine.LambdaFunction.CreateLambda("LAMBDA",
				Instructions,
				CapturedScope ?? new VirtualMachine.ScriptObject(),
				new List<String>(
					Terms.Where(
						(d) => { return d.Type == DeclarationTermType.Term; }
					).Select(
						(d) => { return d.Name; }
					)
					)
				);
			}

			return CachedLambda;
		}

		public VirtualMachine.CodeContext GetEntryPoint(ParseScope DeclarationScope)
		{
			if (Instructions == null) EmitInstructions(DeclarationScope);
			return new VirtualMachine.CodeContext(Instructions, 0);
		}		
	}
}
