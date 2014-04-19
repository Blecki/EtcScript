using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ComplexString : Node
	{
		public List<Node> Pieces;

		public ComplexString(Token Source, List<Node> Pieces)
			: base(Source) 
		{
			this.Pieces = Pieces;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.FindType("COMPLEXSTRING");
			Pieces = new List<Node>(Pieces.Select(s => s.Transform(Scope)).Where(n => n != null));

			if (Pieces.Count == 0) return new StringLiteral(Source, "");

			Pieces.Insert(0, new StringLiteral(Source, ""));

			var lambdaDeclaration = new Declaration();
			lambdaDeclaration.Type = DeclarationType.Lambda;
			lambdaDeclaration.Terms = new List<DeclarationTerm>();
			lambdaDeclaration.ReturnTypeName = "STRING";

			if (Pieces.Count == 1)
			{
				lambdaDeclaration.Body = new LambdaBlock(new Ast.Return(Source) { Value = Pieces[0] });
			}
			else
			{
				var binOp = Pieces[0];
				for (int i = 1; i < Pieces.Count; ++i)
					binOp = new BinaryOperator(Source, VirtualMachine.InstructionSet.ADD, binOp, Pieces[i]);

				lambdaDeclaration.Body = new LambdaBlock(new Ast.Return(Source) { Value = binOp });
			}

			return new Lambda(Source, lambdaDeclaration, "COMPLEXSTRING").Transform(Scope);
		}
	}
}
