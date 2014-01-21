using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct ErrorHandler
    {
        public ScriptObject ParentScope;
        public CodeContext HandlerCode;

		public ErrorHandler(CodeContext HandlerCode, ScriptObject ParentScope)
		{
			this.HandlerCode = HandlerCode;
			this.ParentScope = ParentScope;
		}
    }
}
