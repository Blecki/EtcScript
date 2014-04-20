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

		public MemberAccess(Token Source) : base(Source) { }

		public override Node Transform(ParseScope Scope)
		{
			Object = Object.Transform(Scope);
			if (Object.ResultType.Origin == TypeOrigin.System)
				throw new CompileError("Unimplemented.", Source);
				//return new GenericMemberAccess(Source) { Name = Name, Object = Object }.Transform(Scope);
			else if (Object.ResultType.Origin == TypeOrigin.Script)
				return new StaticMemberAccess(Source) { MemberName = Name, Object = Object }.Transform(Scope);
			else
				throw new CompileError("Can't access members of primitives.", Source);
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
	}
}
