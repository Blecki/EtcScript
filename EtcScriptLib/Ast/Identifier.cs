using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Identifier : Node
	{
		public Token Name;
		public Variable MatchedVariable;

		public Identifier(Token Source) : base(Source) 
		{
			this.Name = Source;
		}

		public override Node Transform(ParseScope Scope)
		{
			if (Name.Type == TokenType.Identifier)
			{
				if (Name.Value.ToUpper() == "TRUE") return new Literal(Source, true);
				else if (Name.Value.ToUpper() == "FALSE") return new Literal(Source, false);
				else MatchedVariable = Scope.FindVariable(Name.Value);
			}
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Name.Type == TokenType.Identifier)
			{
				if (MatchedVariable != null)
					into.AddInstructions("LOAD_PARAMETER NEXT " + Node.WriteOperand(Destination), MatchedVariable.Offset);
				else
					into.AddInstructions("LOOKUP STRING " + Node.WriteOperand(Destination), into.AddString(Name.Value));
			}
			else if (Name.Type == TokenType.String)
				into.AddInstructions("MOVE STRING " + Node.WriteOperand(Destination), into.AddString(Name.Value));
			else if (Name.Type == TokenType.Number)
				into.AddInstructions("MOVE NEXT " + Node.WriteOperand(Destination), Convert.ToSingle(Name.Value));
		}

		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			if (Name.Type == TokenType.Identifier)
			{
				if (MatchedVariable != null)
					into.AddInstructions("STORE_PARAMETER R NEXT", MatchedVariable.Offset);
				else
					into.AddInstructions("SET_VARIABLE R STRING", into.AddString(Name.Value));
			}
			else
				throw new InvalidOperationException();
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Identifier " + Name.Value);
		}
	}
}
