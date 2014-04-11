using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class StaticInvokation : Statement
	{
		public List<Node> Arguments;

		public StaticInvokation(Token Source, List<Node> Arguments) : base(Source) 
		{
			this.Arguments = Arguments;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			throw new InvalidOperationException("Static invokation should have been removed by transformation phase");
		}

		public override Ast.Node Transform(ParseScope Scope)
		{
			var matchingFunction = Scope.FindMacro(Arguments);
			if (matchingFunction == null)
				throw new CompileError("Could not find matching function for static invokation", Source);
			else
			{
				var newParameters = Declaration.GenerateParameterListSyntaxTree(Arguments, matchingFunction.Terms);

				if (matchingFunction.OwnerContextID == Scope.EnvironmentContext.ID && matchingFunction.OwnerContextID != 0)
					return new Ast.JumpCall(Source, matchingFunction, newParameters.Members).Transform(Scope);

				return new Ast.FunctionCall(Source, matchingFunction.MakeInvokableFunction(), newParameters.Members).Transform(Scope);
			}
		}
	}
}
