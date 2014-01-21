using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class CompileError : Exception
	{
		internal CompileError(String message, Token at) :
			base(message + " " + at.ToString())
		{ }

		internal CompileError(String message, Construct at) :
			base(message + " " + FindFirstPhysicalToken(at).ToString())
		{ }

		private static Token FindFirstPhysicalToken(Construct at)
		{
			if (at is Block)
				return FindFirstPhysicalToken((at as Block).Children[0]);
			else
				return (at as Line).Tokens[0];
		}
	}
}