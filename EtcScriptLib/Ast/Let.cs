using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Let : Statement
	{
		public Node LHS;
		public Node Value;

		public Let(Token Source, Node Object, Node Value) : base(Source) 
		{
			this.LHS = Object;
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;
			LHS.IsAssignmentTarget = true;
			LHS = LHS.Transform(Scope);
			if (!(LHS is IAssignable)) throw new CompileError("Assignment target is not an lvalue", Source);
			Value = Value.Transform(Scope);

			var compatibilityResult = Type.AreTypesCompatible(Value.ResultType, (LHS as IAssignable).DestinationType, Scope);
			if (!compatibilityResult.Compatible)
				Type.ThrowConversionError(Value.ResultType, (LHS as IAssignable).DestinationType, Source);

			if (compatibilityResult.ConversionRequired)
				Value = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro, Value)
					.Transform(Scope);

			return (LHS as IAssignable).TransformAssignment(Scope, this, Value);
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Value.Emit(into, OperationDestination.Stack);
			(LHS as IAssignable).EmitAssignment(into);
		}
	}
}
