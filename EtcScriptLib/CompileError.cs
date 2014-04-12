using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class CompileError : Exception
	{
		public CompileError(String message, Token at) :
			base(message + " " + at.ToString())
		{ }

		public CompileError(String message, Iterator<Token> at) :
			base(message + " " + (at.AtEnd() ? "NULL" : at.Next().ToString()))
		{ }

		public CompileError(String message) : base(message) { }
	}
}