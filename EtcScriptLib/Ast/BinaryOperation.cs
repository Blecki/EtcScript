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

		public BinaryOperator(Token Source, VirtualMachine.InstructionSet Instruction,
			Node LHS, Node RHS) : base(Source) 
		{
			this.Instruction = Instruction;
			this.LHS = LHS;
			this.RHS = RHS;
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
			LHS = LHS.Transform(Scope);
			RHS = RHS.Transform(Scope);
			ResultType = LHS.ResultType;
			return this;
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Binary Op - " + Instruction);
			LHS.Debug(depth + 1);
			RHS.Debug(depth + 1);
		}
	}
}
