using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum DeclarationTermType
	{
		Term,
		Keyword
	}

	public enum DeclarationTermRepititionType
	{
		Once,
		Optional,
		OneOrMany,
		NoneOrMany
	}

	public struct DeclarationTerm
	{
		public String Name;
		public DeclarationTermType Type;
		public DeclarationTermRepititionType RepititionType;

		public static bool Matches(DeclarationTerm term, Ast.Node node)
		{
			if (term.Type == DeclarationTermType.Keyword)
			{

				if (node is Ast.Identifier)
					return term.Name == (node as Ast.Identifier).Name.Value.ToUpper();
				return false;
			}

			return true;
		}
	}
}
