using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class BinaryOperator : Node
	{
		public Node LHS;
		public Node RHS;
		public VirtualMachine.InstructionSet Instruction;

		public override void Emit(VirtualMachine.InstructionList into)
		{
			LHS.Emit(into);
			RHS.Emit(into);
			into.AddInstruction(Instruction, VirtualMachine.Operand.POP, VirtualMachine.Operand.POP, VirtualMachine.Operand.PUSH);
		}
	}
}
