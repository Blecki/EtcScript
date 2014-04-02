using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public class ParseScope
	{
		public ParseScope Parent = null;
		public List<Declaration> Macros = new List<Declaration>();
		
		public Declaration FindMacro(List<Ast.Node> Nodes)
		{
			var r = Macros.FirstOrDefault((declaration) =>
				{
					return Declaration.MatchesHeaderPattern(Nodes, declaration.Terms);
				});
			if (r == null && Parent != null) return Parent.FindMacro(Nodes);
			return r;
		}
	}
}
