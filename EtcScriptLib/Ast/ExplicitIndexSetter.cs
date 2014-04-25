using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ExplicitIndexSetter : Node, IAssignable
	{
		public Node Object;
		public Node Index;
		public Declaration Function;

		public ExplicitIndexSetter(Token Source, Declaration Function, Node Object, Node Index)
			: base(Source)
		{
			this.Function = Function;
			this.Object = Object;
			this.Index = Index;
		}

		public override Node Transform(ParseScope Scope)
		{
			ResultType = Function.ReturnType;
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
			arguments.Add(Index);
			arguments.Add(Object);
			arguments.Add(Value);

			return StaticInvokation.CreateCorrectInvokationNode(Source, Scope, Function, arguments);
		}


		public Type DestinationType
		{
			get { return Function.Terms.Last().DeclaredType; }
		}
	}
}
