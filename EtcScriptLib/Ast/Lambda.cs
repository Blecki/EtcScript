using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public class Lambda : Node
	{
		public Declaration Function;
		public String ResultTypeName;

		public Lambda(Token Source, Declaration Function, String ResultTypeName) : base(Source) 
		{
			this.Function = Function;
			this.ResultTypeName = ResultTypeName;
		}

		public override Node Transform(ParseScope Scope)
		{
			Function.ResolveTypes(Scope);
			Function.Transform(Scope.EnvironmentContext.ID);

			//Shift all parameters down by one index to accomodate the RSO used to store captured variables.
			foreach (var variable in Function.DeclarationScope.Variables)
				if (variable.StorageMethod == VariableStorageMethod.Local &&
					variable.Offset < 0)
					variable.Offset -= 1;

			Scope.AddChildLambda(Function);

			if (String.IsNullOrEmpty(ResultTypeName))
				ResultType = Type.Generic;
			else
			{
				ResultType = Scope.FindType(ResultTypeName);
				if (ResultType == null) throw new CompileError("Could not find type '" + ResultTypeName + "'.", Source);
			}

			return this;
		}

		public override void Emit(VirtualMachine.InstructionList into, OperationDestination Destination)
		{
			//Capture variables into an RSO.
			into.AddInstructions("ALLOC_RSO NEXT PUSH", Function.DeclarationScope.CapturedVariables.Count + 3);
			foreach (var capturedVariable in Function.DeclarationScope.CapturedVariables)
			{
				if (capturedVariable.Source.StorageMethod == VariableStorageMethod.Local)
					into.AddInstructions("LOAD_PARAMETER NEXT PUSH", capturedVariable.Source.Offset);
				else if (capturedVariable.Source.StorageMethod == VariableStorageMethod.LambdaCapture)
					into.AddInstructions("LOAD_PARAMETER NEXT R", -3,
						"LOAD_RSO_M R NEXT PUSH", capturedVariable.Source.Offset);
				else
					throw new InvalidProgramException();

				into.AddInstructions("STORE_RSO_M POP PEEK NEXT", capturedVariable.LocalCopy.Offset);
			}
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", 0, Function.DeclarationScope.CapturedVariables.Count);
			Function.Body.CleanupCall = into.Count - 2;
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", 0, Function.DeclarationScope.CapturedVariables.Count + 1);
			Function.Body.CallPoints.Add(into.Count - 2);
			into.AddInstructions("STORE_RSO_M NEXT PEEK NEXT", Function.ActualParameterCount,
				Function.DeclarationScope.CapturedVariables.Count + 2);
			into.AddInstructions("LAMBDA POP " + WriteOperand(Destination));
		}

	}
}
