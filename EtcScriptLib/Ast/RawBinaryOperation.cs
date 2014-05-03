using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class RawBinaryOperator : Node
	{
		public Node LHS;
		public Node RHS;
		public VirtualMachine.InstructionSet Instruction;

		public RawBinaryOperator(Token Source, VirtualMachine.InstructionSet Instruction,
			Node LHS, Node RHS, Type ResultType) : base(Source) 
		{
			this.Instruction = Instruction;
			this.LHS = LHS;
			this.RHS = RHS;
			this.ResultType = ResultType;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			RHS.Emit(into, OperationDestination.Stack);
			LHS.Emit(into, OperationDestination.Stack);
			into.AddInstruction(Instruction, VirtualMachine.Operand.POP, VirtualMachine.Operand.POP, 
				Node.WriteOperand(Destination));
		}

		public override Node Transform(ParseScope Scope)
		{
			return this;
		}

		public override string ToString()
		{
			return LHS.ToString() + " " + Instruction.ToString() + " " + RHS.ToString();
		}
	}
}
