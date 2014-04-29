using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    /// <summary>
    /// A list of rules. Rulebooks stop execution at the first rule that returns RuleResult.Fail.
    /// Only runs rules who's condition returns true.
    /// </summary>
    public class Rulebook
    {
		public List<DeclarationTerm> DeclarationTerms;
        public List<Declaration> Rules = new List<Declaration>();
		public Declaration DefaultValue;
		public Type ResultType;
		public String ResultTypeName;
    }
}
