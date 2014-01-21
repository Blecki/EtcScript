using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum AstType
	{
		Token,
		BinaryOperation,
		FunctionCall,
		Literal,
	}

	public class AstNode
	{
		public string Name;
		public AstType Type;
		public List<AstNode> Children = new List<AstNode>();

		internal static AstNode Create(AstType Type, String Name)
		{
			return new AstNode
			{
				Name = Name,
				Type = Type
			};
		}
	}
}
