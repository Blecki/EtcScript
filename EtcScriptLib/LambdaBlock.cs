using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class LambdaBlock
	{
		internal VirtualMachine.InstructionList Instructions;
		internal int EntryPoint;
		internal Ast.Node Body;
		private VirtualMachine.InvokeableFunction CachedLambda;
		internal List<int> CallPoints = new List<int>();

		public LambdaBlock(Ast.Node Body)
		{
			this.Body = Body;
		}

		public virtual void EmitInstructions(ParseScope DeclarationScope, VirtualMachine.InstructionList Into)
		{
			if (Instructions != null) throw new InvalidOperationException("Instructions should not be emitted twice");

			Body = Body.Transform(DeclarationScope);

			Instructions = Into;
			EntryPoint = Into.Count;
			Instructions.AddInstructions("MOVE F PUSH", "MARK_STACK F");
			Body.Emit(Instructions, Ast.OperationDestination.Discard);
			Instructions.AddInstructions("MOVE NEXT R", 0); //If a function has no return statement, it returns 0.
			var returnJumpPoint = Instructions.Count;
			Instructions.AddInstructions(
				"RESTORE_STACK F",
				"MOVE POP F",
				"CONTINUE POP");

			System.Diagnostics.Debug.Assert(DeclarationScope.Type == ScopeType.Function);
			System.Diagnostics.Debug.Assert(DeclarationScope.ReturnJumpSources != null);

			foreach (var point in DeclarationScope.ReturnJumpSources)
				Instructions[point] = returnJumpPoint;
		}

		public void CacheSystemImplementation(int ArgumentCount, Func<VirtualMachine.ExecutionContext, List<Object>, Object> Implementation)
		{
			CachedLambda = new VirtualMachine.NativeFunction("SYS-LAMBDA", ArgumentCount, Implementation);
		}

		public VirtualMachine.InvokeableFunction GetInvokable(List<DeclarationTerm> Terms)
		{
			if (CachedLambda == null)
			{
				CachedLambda = VirtualMachine.LambdaFunction.CreateLambda("LAMBDA",
				GetEntryPoint(),
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

		public VirtualMachine.InvokeableFunction GetBasicInvokable(List<String> Terms)
		{
			if (CachedLambda == null)
			{
				CachedLambda = VirtualMachine.LambdaFunction.CreateLambda("LAMBDA",
				GetEntryPoint(),
				Terms);
			}

			return CachedLambda;
		}

		public VirtualMachine.CodeContext GetEntryPoint()
		{
			return new VirtualMachine.CodeContext(Instructions, EntryPoint);
		}		
	}
}
