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
				if (Name.Value.ToUpper() == "TRUE") return new Literal(Source, true, "BOOLEAN").Transform(Scope);
				else if (Name.Value.ToUpper() == "FALSE") return new Literal(Source, false, "BOOLEAN").Transform(Scope);
				else MatchedVariable = Scope.FindVariable(Name.Value.ToUpper());
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
					into.AddInstructions("LOAD_PARAMETER NEXT " + Node.WriteOperand(Destination) + " #" + MatchedVariable.Name,
						MatchedVariable.Offset);
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.System)
				{
					into.AddInstructions(
						"MOVE NEXT PUSH", (MatchedVariable as SystemVariable).Implementation,
						"COMPAT_INVOKE NEXT", 1);
					if (Destination != OperationDestination.R)
						into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
				}
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.LambdaCapture)
				{
					into.AddInstructions("LOAD_PARAMETER NEXT R", -3,
						"LOAD_RSO_M R NEXT " + WriteOperand(Destination), MatchedVariable.Offset);
				}
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.Static)
				{
					into.AddInstructions("LOAD_STATIC NEXT " + Node.WriteOperand(Destination), MatchedVariable.Offset);
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
				else if (MatchedVariable.StorageMethod == VariableStorageMethod.Static)
					into.AddInstructions("STORE_STATIC POP NEXT", MatchedVariable.Offset);
				else
					throw new NotImplementedException();
			}
			else
				throw new InvalidOperationException();
		}

		public Node TransformAssignment(ParseScope Scope, Let Let, Node Value) { return Let; }

		public Type DestinationType
		{
			get { return MatchedVariable.DeclaredType; }
		}
	}
}
