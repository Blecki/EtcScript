using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class LocalDeclaration : Statement
	{
		public String Name;
		public Node Value;
		public Variable Variable;
		public String Typename;

		public LocalDeclaration(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			//var existingVariable = Scope.FindVariable(Name);
			//if (existingVariable != null)
			//    throw new CompileError("A variable called '" + Name +
			//        "' can't be defined here because it would hide a variable already defined with that name.", Source);

			if (String.IsNullOrEmpty(Typename))
				ResultType = Type.Generic;
			else
			{
				ResultType = Scope.FindType(Typename);
				if (ResultType == null) throw new CompileError("Could not find type '" + Typename + "'.", Source);
			}

			if (Value != null)
			{
				Value = Value.Transform(Scope);
				
				if (!String.IsNullOrEmpty(Typename))
				{
					var compatibilityResult = Type.AreTypesCompatible(Value.ResultType, ResultType, Scope);
					if (!compatibilityResult.Compatible)
						Type.ThrowConversionError(Value.ResultType, ResultType, Source);

					if (compatibilityResult.ConversionRequired)
						Value = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro, Value)
							.Transform(Scope);
				}
				else //Infer the type of the variable from the expression assigned to it.
					ResultType = Value.ResultType;
			}

			Variable = Scope.NewLocal(Name.ToUpper(), ResultType);
			Variable.DeclaredTypeName = Typename;
			Variable.DeclaredType = ResultType;

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.SetPendingAnnotation(this.ToString());

			if (Value != null)
			{
				Value.Emit(into, OperationDestination.Stack);
			}
			else
			{
				into.AddInstructions("MOVE NEXT PUSH", 0); //Just make room on the stack for it...
			}
		}

		public override string ToString()
		{
			return "var " + Variable.Name + ":" + Variable.DeclaredType.Name +
				(Value == null ? "" : (
					" = " + Value.ToString()));
		}

	}
}
