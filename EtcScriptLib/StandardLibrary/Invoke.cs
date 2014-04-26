using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void Invoke(Environment Environment)
		{
			Environment.AddControl(Control.Create(
				Declaration.Parse("invoke (x)"),
				ControlBlockType.NoBlock,
				(parameters, body) =>
				{
					if (parameters[0] is Ast.StaticInvokation)
						return new Ast.CompatibleCall(parameters[0].Source,
							(parameters[0] as Ast.StaticInvokation).Arguments,
							"GENERIC");
					else throw new CompileError("Argument to invoke must be static invokation", parameters[0].Source);
				}));
		}

	}
}
