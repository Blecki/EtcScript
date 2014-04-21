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

	public enum DeclarationTermRepetitionType
	{
		Once,
		Optional,
		//OneOrMany,
		//NoneOrMany
	}

	public class DeclarationTerm
	{
		public String Name;
		public DeclarationTermType Type;
		public DeclarationTermRepetitionType RepetitionType;
		public String DeclaredTypeName;
		public Type DeclaredType;

		public static bool Matches(DeclarationTerm term, Ast.Node node)
		{
			if (term.Type == DeclarationTermType.Keyword)
			{
				if (node is Ast.Identifier)
					return term.Name == (node as Ast.Identifier).Name.Value.ToUpper();
				else if (node is Ast.MemberAccess.DummyKeyword)
					return term.Name == (node as Ast.MemberAccess.DummyKeyword).Name;
				return false;
			}

			return true;
		}

		public override string ToString()
		{
			if (Type == DeclarationTermType.Keyword)
				return Name + RepetitionMarker(RepetitionType);
			else
				return "(" + Name + ":" + DeclaredTypeName +  ")" + RepetitionMarker(RepetitionType);
		}

		private static string RepetitionMarker(DeclarationTermRepetitionType RepetitionType)
		{
			switch (RepetitionType)
			{
				//case DeclarationTermRepetitionType.NoneOrMany: return "*";
				case DeclarationTermRepetitionType.Once: return "";
				//case DeclarationTermRepetitionType.OneOrMany: return "+";
				case DeclarationTermRepetitionType.Optional: return "?";
				default: throw new InvalidProgramException();
			}
		}
	
	}
}
