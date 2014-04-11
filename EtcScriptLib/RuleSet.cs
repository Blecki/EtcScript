using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class RuleSet
    {
        public List<Rulebook> Rulebooks = new List<Rulebook>();

		public Rulebook FindMatchingRulebook(List<EtcScriptLib.Ast.Node> Invokation)
		{
			foreach (var rulebook in Rulebooks)
				if (EtcScriptLib.Declaration.MatchesHeaderPattern(Invokation, rulebook.DeclarationTerms))
					return rulebook;
			return null;
		}

		public Rulebook FindMatchingRulebook(List<EtcScriptLib.DeclarationTerm> Terms)
		{
			foreach (var rulebook in Rulebooks)
				if (EtcScriptLib.Declaration.AreTermsCompatible(Terms, rulebook.DeclarationTerms))
					return rulebook;
			return null;
		}

		public Rulebook FindBasicRulebook(String Name, int ArgumentCount)
		{
			var nameToken = new EtcScriptLib.Token();
			nameToken.Value = Name;
			nameToken.Type = EtcScriptLib.TokenType.Identifier;
			var astList = new List<EtcScriptLib.Ast.Node>();
			astList.Add(new EtcScriptLib.Ast.Identifier(nameToken));
			for (var i = 0; i < ArgumentCount; ++i)
				astList.Add(new EtcScriptLib.Ast.Identifier(nameToken)); //Just add some dummy arguments to match with.

			return FindMatchingRulebook(astList);
		}

		
    }
}
