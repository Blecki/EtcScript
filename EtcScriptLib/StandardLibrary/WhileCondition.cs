using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void WhileCondition(Environment Environment)
		{
			Environment.AddControl(Control.Create(
				Declaration.Parse("while (x)"),
					ControlBlockType.RequiredBlock,
					(parameters, body) =>
					{
						return new While(parameters[0].Source, parameters[0], body);
					}));
		}

		private class While : Ast.Statement
		{
			public Ast.Node Condition;
			public Ast.Node Body;

			public While(Token Source, Ast.Node Condition, Ast.Node Body)
				: base(Source)
			{
				this.Condition = Condition;
				this.Body = Body;
			}

			public override Ast.Node Transform(ParseScope Scope)
			{
				ResultType = Type.Void;
				Condition = Condition.Transform(Scope);
				Body = Body.Transform(Scope);
				return this;
			}

			public override void Emit(VirtualMachine.InstructionList into, Ast.OperationDestination Destination)
			{
				var LoopStart = into.Count;

				Condition.Emit(into, Ast.OperationDestination.R);

				into.AddInstructions(
					"IF_FALSE R",
					"JUMP NEXT", 0);

				var BreakPoint = into.Count - 1;

				Body.Emit(into, Ast.OperationDestination.Discard);

				into.AddInstructions("JUMP NEXT", LoopStart);

				into[BreakPoint] = into.Count;
			}
		}
	}
}
