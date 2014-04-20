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
			if (Object.ResultType.Origin == TypeOrigin.System)
			{
				//Try to find an access macro for this type.
				var accessArguments = new List<Ast.Node>();
				accessArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "get" }));
				accessArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "0" }) { ResultType = Scope.FindType("STRING") });
				accessArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "from" }));
				accessArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "1" }) { ResultType = Object.ResultType });

				var possibleAccessors = Scope.FindAllPossibleMacroMatches(accessArguments);
				Declaration matchingAccessor = null;
				foreach (var possibleAccessor in possibleAccessors)
					if (System.Object.ReferenceEquals(possibleAccessor.ReturnType, Type.Generic))
					{
						matchingAccessor = possibleAccessor;
						break;
					}

				if (matchingAccessor == null)
					throw new CompileError("No accessor for type " + Object.ResultType.Name + " found.", Source);

				return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, matchingAccessor,
					new List<Node>(new Node[] { new Ast.StringLiteral(Source, Name), Object })).Transform(Scope); ;
			}
			else if (Object.ResultType.Origin == TypeOrigin.Script)
				return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
			else if (Object.ResultType.Origin == TypeOrigin.Primitive)
				throw new CompileError("Can't access members of primitives.", Source);
			else
				throw new CompileError("Impossible.", Source);
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
	}
}
