using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class New : Node
	{
		public String Typename;
		public List<Initializer> Initializers;
		public Declaration Constructor;
		public ParseScope Scope;

		public New(Token Source)
			: base(Source)
		{
		}

		public override Node Transform(ParseScope Scope)
		{
			this.Scope = Scope;
			ResultType = Scope.FindType(Typename);
			if (ResultType == null) throw new CompileError("Unable to find type with name '" + Typename + "'", Source);
			if (ResultType.Origin != TypeOrigin.Script) 
				throw new CompileError("New cannot be used with primitive or system types", Source);

			var constructorArguments = DummyArguments(Keyword("CONSTRUCT"), Term(ResultType));
			Constructor = Scope.FindAllPossibleMacroMatches(constructorArguments).Where(d =>
				ExactDummyMatch(d.Terms, constructorArguments)).FirstOrDefault();
			
			if (Initializers != null)
			{
				foreach (var initializer in Initializers)
				{
					initializer.ObjectType = ResultType;
					initializer.Transform(Scope);
				}
			}

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			into.AddInstructions("ALLOC_RSO NEXT PUSH", ResultType.Size);
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", ResultType.ID, 0); //Store type id

			if (Constructor != null)
			{
				if (Constructor.OwnerContextID == Scope.EnvironmentContext.ID && Constructor.OwnerContextID != 0)
				{
					into.AddInstructions("CALL NEXT #" + Constructor.DescriptiveHeader, 0);
					Constructor.Body.CallPoints.Add(into.Count - 1);
				}
				else
				{
					into.AddInstructions("STACK_INVOKE NEXT", Constructor.MakeInvokableFunction());
				}
			}

			if (Initializers != null)
			{
				foreach (var initializer in Initializers)
					initializer.Emit(into, OperationDestination.Discard);

			}

			if (Destination != OperationDestination.Stack)
				into.AddInstructions("MOVE POP " + WriteOperand(Destination));
		}
	}
}
