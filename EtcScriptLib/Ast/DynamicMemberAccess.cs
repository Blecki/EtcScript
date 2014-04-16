using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class DynamicMemberAccess : Node, IAssignable
	{
		public Node Object;
		public String Name;
		public Node DefaultValue;

		public DynamicMemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Generic;
			Object = Object.Transform(Scope);
			if (DefaultValue != null) DefaultValue = DefaultValue.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (DefaultValue == null) 
				throw new CompileError("Dynamic member access must have a default value when used as an rvalue", Source);
			
			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("DYN_LOOKUP_MEMBER STRING R PUSH", into.AddString(Name));

			into.AddInstructions(
				"IF_TRUE R",
				"JUMP NEXT", 0);

			var jumpSource = into.Count - 1;
			DefaultValue.Emit(into, OperationDestination.Top);
			into[jumpSource] = into.Count;

			if (Destination != OperationDestination.Stack)
				into.AddInstructions("MOVE POP " + Node.WriteOperand(Destination));
		}

		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			if (DefaultValue != null)
				throw new CompileError("Dynamic member access can not have a default value when used as an lvalue", Source);

			Object.Emit(into, OperationDestination.R);
			into.AddInstructions("DYN_SET_MEMBER POP STRING R", into.AddString(Name));			
		}
	}
}
