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
			LHS = LHS.Transform(Scope);
			if (!(LHS is IAssignable)) throw new CompileError("Assignment target is not an lvalue", Source);
			Value = Value.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			Value.Emit(into, OperationDestination.Stack);
			(LHS as IAssignable).EmitAssignment(into);

			//if (LHS is MemberAccess)
			//{
			//    (LHS as MemberAccess).Object.Emit(into, OperationDestination.Stack);
			//    Value.Emit(into, OperationDestination.R);
			//    if ((LHS as MemberAccess).IsDynamicAccess)
			//        into.AddInstructions("DYN_SET_MEMBER R STRING POP", into.AddString((LHS as MemberAccess).Name));
			//    else
			//        into.AddInstructions("SET_MEMBER R STRING POP", into.AddString((LHS as MemberAccess).Name));
			//}
			//else if (LHS is Identifier)
			//{
			//    Value.Emit(into, OperationDestination.R);
			//    (LHS as Identifier).EmitAssignment(into);
			//}
			//else
			//    throw new InvalidProgramException();
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Assign");
			Value.Debug(depth + 1);
			Console.WriteLine(new String(' ', depth * 3) + "To");
			LHS.Debug(depth + 1);
		}
	}
}
