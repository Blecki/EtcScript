using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct ErrorHandler
    {
        public ExecutionLocation HandlerCode;

		public ErrorHandler(ExecutionLocation HandlerCode)
		{
			this.HandlerCode = HandlerCode;
		}
    }
}
