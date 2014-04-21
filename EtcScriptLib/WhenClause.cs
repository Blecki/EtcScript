using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class WhenClause : LambdaBlock
	{
		public WhenClause(Ast.Node Expression) : base(Expression)
		{}

		public override void EmitInstructions(ParseScope DeclarationScope, VirtualMachine.InstructionList Into)
		{
			if (Instructions != null) throw new InvalidOperationException("Instructions should not be emitted twice");

			CleanupPoint = Into.Count;

			if (CleanupCall >= 0)
				Into[CleanupCall] = CleanupPoint;

			Into.AddInstructions("CLEANUP NEXT #Cleanup when clause for " + Name, 
				DeclarationScope.Owner.ActualParameterCount, "CONTINUE POP");

			Instructions = Into;
			EntryPoint = Into.Count;
			Instructions.AddInstructions("MOVE F PUSH #Enter " + Name, "MARK_STACK F");
			Body.Emit(Instructions, Ast.OperationDestination.R);
			Instructions.AddInstructions(
				"RESTORE_STACK F",
				"MOVE POP F",
				"CONTINUE POP");
		}
		
	}
}
