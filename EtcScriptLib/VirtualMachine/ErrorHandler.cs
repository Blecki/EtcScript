using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct ErrorHandler
    {
        public CodeContext HandlerCode;

		public ErrorHandler(CodeContext HandlerCode)
		{
			this.HandlerCode = HandlerCode;
		}
    }
}
