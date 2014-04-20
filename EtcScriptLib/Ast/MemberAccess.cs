using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class MemberAccess : Node
	{
		public Node Object;
		public String Name;

		public MemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);

			if (IsAssignmentTarget)
			{
				if (Object.ResultType.Origin == TypeOrigin.System)
				{
					//Try to find an access macro for this type.
					var setterArguments = new List<Ast.Node>();
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "set" }));
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "0" }) { ResultType = Scope.FindType("STRING") });
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "on" }));
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "1" }) { ResultType = Object.ResultType });
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "to" }));
					setterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "2" }) { ResultType = Type.Generic });

					var possibleSetters = Scope.FindAllPossibleMacroMatches(setterArguments);
					Declaration matchingSetter = null;
					foreach (var possibleSetter in possibleSetters)
						if (System.Object.ReferenceEquals(possibleSetter.ReturnType, Type.Void))
						{
							matchingSetter = possibleSetter;
							break;
						}

					if (matchingSetter == null)
						throw new CompileError("No setter for type " + Object.ResultType.Name + " found.", Source);

					return new GenericSetter(Source, matchingSetter, Name, Object).Transform(Scope);
				}
				else if (Object.ResultType.Origin == TypeOrigin.Script)
					return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
				else
					throw new CompileError("Can't assign to this target", Source);
			}
			else
			{
				if (Object.ResultType.Origin == TypeOrigin.System)
				{
					//Try to find an access macro for this type.
					var getterArguments = new List<Ast.Node>();
					getterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "get" }));
					getterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "0" }) { ResultType = Scope.FindType("STRING") });
					getterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "from" }));
					getterArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "1" }) { ResultType = Object.ResultType });

					var possibleGetters = Scope.FindAllPossibleMacroMatches(getterArguments);
					Declaration matchingGetter = null;
					foreach (var possibleGetter in possibleGetters)
						if (System.Object.ReferenceEquals(possibleGetter.ReturnType, Type.Generic))
						{
							matchingGetter = possibleGetter;
							break;
						}

					if (matchingGetter == null)
						throw new CompileError("No accessor for type " + Object.ResultType.Name + " found.", Source);

					return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingGetter,
						new List<Node>(new Node[] { new Ast.StringLiteral(Source, Name), Object })).Transform(Scope); ;
				}
				else if (Object.ResultType.Origin == TypeOrigin.Script)
					return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
				else if (Object.ResultType.Origin == TypeOrigin.Primitive)
					throw new CompileError("Can't access members of primitives.", Source);
				else
					throw new CompileError("Impossible.", Source);
			}
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
	}
}
