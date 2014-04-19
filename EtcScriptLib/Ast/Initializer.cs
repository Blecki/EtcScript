using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Initializer : Node
	{
		public String MemberName;
		public Node Value;
		public Type ObjectType;
		private Variable Member;

		public Initializer(Token Source, String MemberName, Node Value) : base(Source) 
		{
			this.MemberName = MemberName;
			this.Value = Value;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Void;

			Value = Value.Transform(Scope);
			Member = ObjectType.FindMember(MemberName);
			if (Member == null) throw new CompileError("Unable to find member '" + MemberName + "' of " + ObjectType.Name);

			var compatibilityResult = Type.AreTypesCompatible(Value.ResultType, Member.DeclaredType, Scope);
			if (!compatibilityResult.Compatible)
				Type.ThrowConversionError(Value.ResultType, Member.DeclaredType, Source);

			if (compatibilityResult.ConversionRequired)
				Value = Type.CreateConversionInvokation(Scope, compatibilityResult.ConversionMacro, Value).Transform(Scope);

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			//Assumes the RSO is on the top of the stack.
			Value.Emit(into, OperationDestination.R);
			into.AddInstructions("STORE_RSO_M R PEEK NEXT", Member.Offset);
		}
	}
}
