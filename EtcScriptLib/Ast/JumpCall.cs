using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class JumpCall : Statement
	{
		public List<Node> Arguments;
		public Declaration Function;

		public JumpCall(Token Source, Declaration Function, List<Node> Arguments) : base(Source) 
		{
			this.Function = Function;
			this.Arguments = Arguments;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Function.ReturnType;
			Arguments = new List<Node>(Arguments.Select(n => n.Transform(Scope)));

			//Check types
			int argumentIndex = 0;
			foreach (var term in Function.Terms)
				if (term.Type == DeclarationTermType.Term)
				{
					var compatibilityResult = Type.AreTypesCompatible(Arguments[argumentIndex].ResultType, term.DeclaredType, Scope);
					if (!compatibilityResult.Compatible)
						Type.ThrowConversionError(Arguments[argumentIndex].ResultType, term.DeclaredType, Source);

					if (compatibilityResult.ConversionRequired)
						Arguments[argumentIndex] = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro,
							Arguments[argumentIndex]).Transform(Scope);

					argumentIndex += 1;
				}

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			foreach (var arg in Arguments)
				arg.Emit(into, OperationDestination.Stack);
			into.AddInstructions("CALL NEXT", 0);
			Function.Body.CallPoints.Add(into.Count - 1);
			into.AddInstructions("CLEANUP NEXT", Arguments.Count);
			if (Destination != OperationDestination.R && Destination != OperationDestination.Discard)
				into.AddInstructions("MOVE R " + Node.WriteOperand(Destination));
		}

		public override void Debug(int depth)
		{
			Console.Write(new String(' ', depth * 3));
			Console.WriteLine("Static Call");
			foreach (var arg in Arguments)
				arg.Debug(depth + 1);
		}
	}
}
