using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class MacroFunction : InvokeableFunction
    {
		private Declaration Declaration;

		public MacroFunction(Declaration Declaration)
		{
			this.Declaration = Declaration;
		}

        public override InvokationResult Invoke(ExecutionContext context, List<Object> arguments)
        {
			return Declaration.MakeInvokableFunction().Invoke(context, arguments);
        }
    }
}
