using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ComplexString : Node
	{
		public List<Node> Pieces;
		public Declaration Function;
		private bool HasBeenTransformed = false;

		public ComplexString(Token Source, List<Node> Pieces)
			: base(Source) 
		{
			this.Pieces = Pieces;
		}

		public override Node Transform(ParseScope Scope)
		{
			if (HasBeenTransformed) return this;

			ResultType = Scope.FindType("COMPLEXSTRING");

			Function = new Declaration();
			Function.Type = DeclarationType.Lambda;
			Function.Terms = new List<DeclarationTerm>();
			Function.ReturnTypeName = "STRING";
			Function.ReturnType = Scope.FindType("STRING");
			Function.DeclarationScope = Scope.Push(ScopeType.Function);
			Function.DeclarationScope.Owner = Function;

			Pieces = new List<Node>(Pieces.Select(s => s.Transform(Function.DeclarationScope)).Where(n => n != null));

			if (Pieces.Count < 1)
				Pieces.Insert(0, new StringLiteral(Source, "").Transform(Function.DeclarationScope));
			
			if (Pieces.Count == 1)
			{
				Function.Body = new LambdaBlock(new Ast.Return(Source) { Value = Pieces[0] });
				Function.Body.Transform(Function.DeclarationScope);
			}
			else
			{
				var stringType = Scope.FindType("STRING");

				var binOp = Convert(Pieces[0], stringType, Scope);
				for (int i = 1; i < Pieces.Count; ++i)
					binOp = new RawBinaryOperator(Source, VirtualMachine.InstructionSet.ADD, binOp, 
						Convert(Pieces[i], stringType, Function.DeclarationScope), stringType);

				Function.Body = new LambdaBlock(new Ast.Return(Source) { Value = binOp });
				Function.Body.Transform(Function.DeclarationScope);
			}

			Scope.AddChildLambda(Function);

			HasBeenTransformed = true;
			return this;
		}

		private Node Convert(Node Node, Type StringType, ParseScope Scope)
		{
			var conversionInfo = Type.AreTypesCompatible(Node.ResultType, StringType, Scope);
			if (!conversionInfo.Compatible)
				Type.ThrowConversionError(Node.ResultType, StringType, Source);

			if (conversionInfo.ConversionRequired)
				return Type.CreateConversionInvokation(Scope, conversionInfo.ConversionMacro, Node).Transform(Scope);
			return Node;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			//Capture variables into an RSO.
			into.AddInstructions("ALLOC_RSO NEXT PUSH", Function.DeclarationScope.CapturedVariables.Count + 3);
			foreach (var capturedVariable in Function.DeclarationScope.CapturedVariables)
			{
				if (capturedVariable.Source.StorageMethod == VariableStorageMethod.Local)
					into.AddInstructions("LOAD_PARAMETER NEXT PUSH", capturedVariable.Source.Offset);
				else if (capturedVariable.Source.StorageMethod == VariableStorageMethod.LambdaCapture)
					into.AddInstructions("LOAD_PARAMETER NEXT R", -3,
						"LOAD_RSO_M R NEXT PUSH", capturedVariable.Source.Offset);
				else
					throw new InvalidProgramException();

				into.AddInstructions("STORE_RSO_M POP PEEK NEXT", capturedVariable.LocalCopy.Offset);
			}
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", 0, Function.DeclarationScope.CapturedVariables.Count);
			Function.Body.CleanupCall = into.Count - 2;
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", 0, Function.DeclarationScope.CapturedVariables.Count + 1);
			Function.Body.CallPoints.Add(into.Count - 2);
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", Function.ActualParameterCount,
				Function.DeclarationScope.CapturedVariables.Count + 2);
			into.AddInstructions("LAMBDA POP " + WriteOperand(Destination));
		}
	}
}
