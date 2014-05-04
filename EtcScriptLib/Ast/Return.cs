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

				if (Scope.OwnerFunction.Type == DeclarationType.Rule 
					&& Scope.OwnerFunctionReturnType.Name != "RULE-RESULT")
				{
					Value = new RuleResultNode(Source, Value).Transform(Scope);
				}
				else
				{
					var conversionInfo = Type.AreTypesCompatible(Value.ResultType, Scope.OwnerFunctionReturnType, Scope);
					if (!conversionInfo.Compatible)
						Type.ThrowConversionError(Value.ResultType, Scope.OwnerFunctionReturnType, Source);

					if (conversionInfo.ConversionRequired)
						Value = Type.CreateConversionInvokation(Scope, conversionInfo.ConversionMacro, Value).Transform(Scope);
				}
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

		public override string ToString()
		{
			return "return " + (Value == null ? "" : Value.ToString());
		}
	}

	public class RuleResultNode : Node
	{
		public Node Value;

		public RuleResultNode(Token Source, Node Value)
			: base(Source)
		{
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Scope.OwnerFunctionReturnType;
			if (Value.ResultType.Name != "RULE-NEVERMIND")
			{
				var conversionInfo = Type.AreTypesCompatible(Value.ResultType, Scope.OwnerFunctionReturnType, Scope);
				if (!conversionInfo.Compatible)
					Type.ThrowConversionError(Value.ResultType, Scope.OwnerFunctionReturnType, Source);

				if (conversionInfo.ConversionRequired)
					Value = Type.CreateConversionInvokation(Scope, conversionInfo.ConversionMacro, Value).Transform(Scope);
			}
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList Instructions, OperationDestination Destination)
		{
			if (Value.ResultType.Name == "RULE-NEVERMIND")
			{
				Instructions.AddInstructions("ALLOC_RSO NEXT R # Return NEVERMIND", 1);
				Instructions.AddInstructions("STORE_RSO_M NEXT R NEXT", 0, 0);
			}
			else
			{
				Instructions.AddInstructions("ALLOC_RSO NEXT R # Return an actual value", 2);
				Instructions.AddInstructions("STORE_RSO_M NEXT R NEXT", 1, 0);
				Value.Emit(Instructions, OperationDestination.Stack);
				Instructions.AddInstructions("STORE_RSO_M POP R NEXT", 1);
			}			
		}
	}
}
