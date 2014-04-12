using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Identifier : Node, IAssignable
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

				if (MatchedVariable != null) ResultType = MatchedVariable.DeclaredType;
				else ResultType = Type.Generic;
			}
			else if (Name.Type == TokenType.String)
			{
				ResultType = Scope.FindType("string");
			}
			else if (Name.Type == TokenType.Number)
			{
				ResultType = Scope.FindType("number");
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
					into.AddInstructions("STORE_PARAMETER POP NEXT", MatchedVariable.Offset);
				else
					into.AddInstructions("SET_VARIABLE POP STRING", into.AddString(Name.Value));
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
