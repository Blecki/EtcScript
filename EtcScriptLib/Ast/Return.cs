using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Return : Statement
	{
		public Node Value;
		public ParseScope DeclarationScope;

		public Return(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;
			if (Value != null)
			{
				Value = Value.Transform(Scope);
				var conversionInfo = Type.AreTypesCompatible(Value.ResultType, Scope.OwnerFunctionReturnType, Scope);
				if (!conversionInfo.Compatible)
					Type.ThrowConversionError(Value.ResultType, Scope.OwnerFunctionReturnType, Source);

				if (conversionInfo.ConversionRequired)
					Value = Type.CreateConversionInvokation(Scope, conversionInfo.ConversionMacro, Value).Transform(Scope);
			}
			else
			{
				if (!Object.ReferenceEquals(Scope.OwnerFunctionReturnType, Type.Void))
					throw new CompileError("This function must return a value", Source);
			}
			DeclarationScope = Scope;
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{

			if (Value != null)
				Value.Emit(into, OperationDestination.R);
			else
				into.AddInstructions("MOVE NEXT R", 0);

			into.AddInstructions("JUMP NEXT", 0);
			DeclarationScope.RecordReturnJumpSource(into.Count - 1);
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Return");
			if (Value != null) Value.Debug(depth + 1);
		}
	}
}
