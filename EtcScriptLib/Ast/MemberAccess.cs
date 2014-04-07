using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class MemberAccess : Node
	{
		public Node Object;
		public String Name;
		public Node DefaultValue = null;
		public bool IsDynamicAccess = false;

		public MemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);
			if (DefaultValue != null) DefaultValue = DefaultValue.Transform(Scope);
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			if (IsDynamicAccess)
			{
				Object.Emit(into, OperationDestination.R);
				into.AddInstructions("DYN_LOOKUP_MEMBER STRING R PUSH", into.AddString(Name));

				if (DefaultValue != null)
				{

					into.AddInstructions(
					"IF_TRUE R",
					"JUMP NEXT", 0);

					var jumpSource = into.Count - 1;

					DefaultValue.Emit(into, OperationDestination.Top);

					into[jumpSource] = into.Count;
				}

				if (Destination != OperationDestination.Top)
					into.AddInstructions("MOVE POP " + Node.WriteOperand(Destination));
			}
			else
			{
				Object.Emit(into, OperationDestination.R);
				into.AddInstructions("LOOKUP_MEMBER STRING R " + Node.WriteOperand(Destination), into.AddString(Name));
			}
		}

		public override void Debug(int depth)
		{
			Console.WriteLine(new String(' ', depth * 3) + "Fetch " + Name + " from ");
			Object.Debug(depth + 1);
		}
	}
}
