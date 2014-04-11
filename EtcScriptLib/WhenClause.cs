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

		public override void  EmitInstructions(ParseScope DeclarationScope, VirtualMachine.InstructionList Into)
{
 	
			if (Instructions != null) throw new InvalidOperationException("Instructions should not be emitted twice");
			Instructions = Into;
			EntryPoint = Into.Count;

			Body = Body.Transform(DeclarationScope);

			Body.Emit(Instructions, Ast.OperationDestination.R);
			Instructions.AddInstructions("CONTINUE POP");
		}
		
	}
}
