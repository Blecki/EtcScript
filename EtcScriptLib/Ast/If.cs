using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class If : Statement
	{
		public Node Header;
		public Node ThenBlock;
		public Node ElseBlock;

		public If(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;
			Header = Header.Transform(Scope);
			ThenBlock = ThenBlock.Transform(Scope);
			if (ElseBlock != null) ElseBlock = ElseBlock.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Header.Emit(into, OperationDestination.R);
			into.AddInstructions("IF_FALSE R", "JUMP NEXT", 0);
			var jumpFrom = into.Count - 1;
			ThenBlock.Emit(into, OperationDestination.Discard);
			if (ElseBlock != null)
			{
				into.AddInstructions("JUMP NEXT", 0);
				into[jumpFrom] = into.Count;
				jumpFrom = into.Count - 1;
				ElseBlock.Emit(into, OperationDestination.Discard);
			}
			into[jumpFrom] = into.Count;
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("If");
			Header.Debug(depth + 1);
			Console.WriteLine(new String(' ', depth * 3) + "Then:");
			ThenBlock.Debug(depth + 1);
			if (ElseBlock != null)
			{
				Console.WriteLine(new String(' ', depth * 3) + "Else:");
				ElseBlock.Debug(depth + 1);
			}
		}
	}
}
