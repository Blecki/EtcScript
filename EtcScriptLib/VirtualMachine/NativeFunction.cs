using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class NativeFunction : InvokeableFunction
    {
        public String Name = null;
		public int ParameterCount = 0;
		private bool StackCallable = false;
        public Func<ExecutionContext, List<Object>, Object> NativeImplementation { get; private set; }

        public NativeFunction(
            String Name,
			int ParameterCount,
            Func<ExecutionContext, List<Object>, Object> NativeImplementation)
        {
            this.Name = Name;
			this.ParameterCount = ParameterCount;
            this.NativeImplementation = NativeImplementation;
			StackCallable = true;
        }

        public NativeFunction(Func<ExecutionContext, List<Object>, Object> NativeImplementation)
        {
            this.NativeImplementation = NativeImplementation;
			StackCallable = false;
        }

        public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
        {
            try
            {
                var result = NativeImplementation.Invoke(context, arguments.GetRange(1, arguments.Count - 1));
                VirtualMachine.SetOperand(Operand.R, result, context);
                return InvokationResult.Success;
            }
            catch (Exception e)
            {
                return InvokationResult.Failure(e.Message + e.StackTrace);
            }
        }

		public override bool IsStackInvokable
		{
			get
			{
				return StackCallable;
			}
		}

		public override void StackInvoke(ExecutionContext context)
		{
			var arguments = new List<Object>();
			for (int i = -1 - ParameterCount; i < -1; ++i) //Last argument is at -2.
				arguments.Add(context.Stack[context.Stack.Count + i]);

			var result = NativeImplementation.Invoke(context, arguments);
			VirtualMachine.SetOperand(Operand.R, result, context);
		}
    }
}
