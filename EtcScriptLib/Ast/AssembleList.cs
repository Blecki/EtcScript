using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class AssembleList : Node
	{
		public List<Node> Members = new List<Node>();
		public AssembleList(Token Source, List<Node> Members)
			: base(Source)
		{
			this.Members = Members;
		}

		public override Node Transform(ParseScope Scope)
		{
			Members = new List<Node>(Members.Select(s => s.Transform(Scope)));
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList Instructions, OperationDestination Destination)
		{
			Instructions.AddInstructions("EMPTY_LIST PUSH");

			foreach (var member in Members)
			{
				member.Emit(Instructions, OperationDestination.R);
				Instructions.AddInstructions("APPEND R PEEK PEEK");
			}

			if (Destination == OperationDestination.Discard)
				Instructions.AddInstructions("MOVE POP");
			else if (Destination != OperationDestination.Top)
				Instructions.AddInstructions("MOVE POP " + WriteOperand(Destination));			
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Assemble List");
			foreach (var member in Members)
				member.Debug(depth + 1);
		}
	}
}
