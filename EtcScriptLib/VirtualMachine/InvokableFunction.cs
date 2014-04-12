using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct InvokationResult
    {
        public bool InvokationSucceeded;
        public String ErrorMessage;

        public InvokationResult(bool InvokationSucceeded, String ErrorMessage)
        {
            this.InvokationSucceeded = InvokationSucceeded;
            this.ErrorMessage = ErrorMessage;
        }

        public static InvokationResult Success { get { return new InvokationResult(true, ""); } }
        public static InvokationResult Failure(String Message) { return new InvokationResult(false, Message); }
    }

    public class InvokeableFunction
    {
        public virtual InvokationResult Invoke(ExecutionContext context, List<Object> arguments) { throw new NotImplementedException(); }
		public virtual void StackInvoke(ExecutionContext context) { throw new NotImplementedException(); }
		public virtual bool IsStackInvokable { get { return false; } }
		public virtual Type ReturnType { get { return Type.Void; } }
    }
}
