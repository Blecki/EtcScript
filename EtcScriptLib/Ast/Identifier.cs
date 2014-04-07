using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Identifier : Node
	{
		public Token Name;

		public Identifier(Token Source) : base(Source) 
		{
			this.Name = Source;
		}

		public override Node Transform(ParseScope Scope)
		{
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Name.Type == TokenType.Identifier)
				into.AddInstructions("LOOKUP STRING " + Node.WriteOperand(Destination), into.AddString(Name.Value));
			else if (Name.Type == TokenType.String)
				into.AddInstructions("MOVE STRING " + Node.WriteOperand(Destination), into.AddString(Name.Value));
			else if (Name.Type == TokenType.Number)
				into.AddInstructions("MOVE NEXT " + Node.WriteOperand(Destination), Convert.ToSingle(Name.Value));
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Identifier " + Name.Value);
		}
	}
}
