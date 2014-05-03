using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class StaticMemberAccess : Node, IAssignable
	{
		public Node Object;
		public String MemberName;
		public Variable MemberVariable;

		public StaticMemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);
			if (Object.ResultType.Origin == TypeOrigin.System) throw new InvalidOperationException();
			MemberVariable = Object.ResultType.FindMember(MemberName);
			if (MemberVariable == null) throw new CompileError("Could not find member '" + MemberName + "' on type '" +
				Object.ResultType.Name + "'.", Source);
			this.ResultType = MemberVariable.DeclaredType;
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("LOAD_RSO_M R NEXT " + WriteOperand(Destination), MemberVariable.Offset);
		}
		
		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("STORE_RSO_M POP R NEXT", MemberVariable.Offset);
		}

		public Node TransformAssignment(ParseScope Scope, Let Let, Node Value) { return Let; }

		public Type DestinationType
		{
			get { return MemberVariable.DeclaredType; }
		}

		public override string ToString()
		{
			return Object.ToString() + "." + MemberVariable.Name;
		}
	}
}
