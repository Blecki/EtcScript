using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class GenericSetter : Node, IAssignable
	{
		public Node Object;
		public String MemberName;
		public Declaration Function;

		public GenericSetter(Token Source, Declaration Function, String MemberName, Node Object) : base(Source) 
		{
			this.Function = Function;
			this.Object = Object;
			this.MemberName = MemberName;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Type.Generic;
			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException();
		}
		
		public void EmitAssignment(VirtualMachine.InstructionList into)
		{
			throw new InvalidOperationException();
		}

		public Node TransformAssignment(ParseScope Scope, Let Let, Node Value)
		{
			var arguments = new List<Node>();
			arguments.Add(new StringLiteral(Source, MemberName));
			arguments.Add(Object);
			arguments.Add(Value);

			return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, Function, arguments);
		}
	}
}
