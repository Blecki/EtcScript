using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public enum OperationDestination
	{
		R,
		Stack,
		Discard,
		Top,
	}

	public class Node
	{
		public static VirtualMachine.Operand WriteOperand(OperationDestination Destination)
		{
			switch (Destination)
			{
				case OperationDestination.R:
					return VirtualMachine.Operand.R;
				case OperationDestination.Stack:
					return VirtualMachine.Operand.PUSH;
				case OperationDestination.Top:
					return VirtualMachine.Operand.PEEK;
				case OperationDestination.Discard:
				default:
					throw new InvalidOperationException();
			}
		}
		public static VirtualMachine.Operand ReadOperand(OperationDestination Destination)
		{
			switch (Destination)
			{
				case OperationDestination.R:
					return VirtualMachine.Operand.R;
				case OperationDestination.Stack:
					return VirtualMachine.Operand.POP;
				case OperationDestination.Top:
					return VirtualMachine.Operand.PEEK;
				case OperationDestination.Discard:
				default:
					throw new InvalidOperationException();
			}
		}

		public Token Source;

		public Node(Token Source)
		{
			this.Source = Source;
		}

		public virtual void Emit(VirtualMachine.InstructionList Instructions, OperationDestination Destination)
		{
			throw new NotImplementedException();
		}

		public virtual void Debug(int depth)
		{
			throw new NotImplementedException();
		}

		public virtual Node Transform(ParseScope Scope)
		{
			throw new NotImplementedException();
		}
	}
}
