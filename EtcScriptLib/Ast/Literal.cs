using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Literal : Node
	{
		public Object Value;

		public Literal(Token Source, Object Value) : base(Source) 
		{
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Generic;
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.AddInstructions("MOVE NEXT " + Node.WriteOperand(Destination), Value);
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Literal " + Value);
		}
	}
}
