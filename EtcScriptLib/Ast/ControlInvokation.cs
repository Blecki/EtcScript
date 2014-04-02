using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class ControlInvokation : Statement
	{
		public Control Control;
		public List<Node> Arguments;
		public Node Body;

		public ControlInvokation(Token Source, Control Control, List<Node> Arguments, Node Body) : base(Source) 
		{
			this.Control = Control;
			this.Arguments = Arguments;
			this.Body = Body;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException("Control invokation should have been removed by transformation phase");
		}

		public override Ast.Node Transform(ParseScope Scope)
		{
			var r = Control.TransformationFunction(
				Declaration.GenerateParameterListSyntaxTree(Arguments, Control.DeclarationTerms).Members,
				Body);
			return r.Transform(Scope);
		}
	}
}
