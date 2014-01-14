using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot.VirtualMachine
{
	public class OverloadedReflectionFunction : InvokeableFunction
	{
		internal Object ThisObject;
		internal String MethodName;

		public OverloadedReflectionFunction(Object ThisObject, String MethodName)
		{
			this.ThisObject = ThisObject;
			this.MethodName = MethodName;
		}

		public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
		{
			var trimmedArguments = arguments.GetRange(1, arguments.Count - 1);
			var argumentTypes = trimmedArguments.Select((obj) => obj.GetType()).ToArray();

			var method = ThisObject.GetType().GetMethod(MethodName, argumentTypes);
			if (method == null)
			{
				var errorMessage = String.Format("Could not find overload for {0} that takes argument types {1} on {2}.",
					MethodName,
					String.Join(", ", argumentTypes.Select(t => t.Name)),
					ThisObject.GetType().Name);
				return InvokationResult.Failure(errorMessage);
			}

			var result = method.Invoke(ThisObject, trimmedArguments.ToArray());

			VirtualMachine.SetOperand(Operand.R, result, context);
			return InvokationResult.Success;
		}
	}
}
