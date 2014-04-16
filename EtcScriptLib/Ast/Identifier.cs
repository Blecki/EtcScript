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
				if (Name.Value.ToUpper() == "TRUE") return new Literal(Source, true, "BOOLEAN");
				else if (Name.Value.ToUpper() == "FALSE") return new Literal(Source, false, "BOOLEAN");
				else MatchedVariable = Scope.FindVariable(Name.Value);
				if (MatchedVariable == null) throw new CompileError("Could not find variable named '" + Name.Value + "'.", Source);
				ResultType = MatchedVariable.DeclaredType;
			}
			else if (Name.Type == TokenType.String)
				return new StringLiteral(Source, Name.Value).Transform(Scope);
			else if (Name.Type == TokenType.Number)
			{
				ResultType = Scope.FindType("NUMBER");
			}

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (Name.Type == TokenType.Identifier)
			{
				if (MatchedVariable.StorageMethod == VariableStorageMethod.Local)
					into.AddInstructions("LOAD_PARAMETER NEXT " + Node.WriteOperand(Destination), MatchedVariable.Offset);
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.System)
				{
					into.AddInstructions(
						"EMPTY_LIST PUSH",
						"APPEND NEXT PEEK PEEK", (MatchedVariable as SystemVariable).Implementation,
						"INVOKE POP");
					if (Destination != OperationDestination.R)
						into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
				}
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.LambdaCapture)
				{
					into.AddInstructions("LOAD_PARAMETER NEXT R", -3,
						"LOAD_RSO_M R NEXT " + WriteOperand(Destination), MatchedVariable.Offset);
				}
				else
					throw new NotImplementedException();
			}
			else if (Name.Type == TokenType.String)
				throw new InvalidOperationException();
			else if (Name.Type == TokenType.Number)
				into.AddInstructions("MOVE NEXT " + Node.WriteOperand(Destination), Convert.ToSingle(Name.Value));
		}

		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			if (Name.Type == TokenType.Identifier)
			{
				if (MatchedVariable.StorageMethod == VariableStorageMethod.Local)
					into.AddInstructions("STORE_PARAMETER POP NEXT", MatchedVariable.Offset);
				else
					throw new NotImplementedException();
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
