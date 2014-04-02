using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace EtcScriptTests
{
    [TestFixture]
    public class PatternMatching
    {
		private static List<EtcScriptLib.Ast.Node> ParseInvokation(String script)
		{
			var parseContext = EtcScriptLib.Compile.GetDefaultOperators();
			var tokenIterator = new EtcScriptLib.TokenStream(new EtcScriptLib.Compile.StringIterator(script), parseContext);
			return EtcScriptLib.Parse.ParseStaticInvokationStatement(tokenIterator, parseContext);
		}

		private static bool MatchesDeclaration(String header, String script, bool debug = false)
		{
			var declaration = EtcScriptLib.Declaration.Parse(header);
			var invokation = ParseInvokation(script);

			if (debug)
			{
				Console.WriteLine("Header: " + header);
				foreach (var term in declaration.Terms)
					Console.WriteLine("  " + term.Name + " " + term.Type + " " + term.RepititionType);
				Console.WriteLine("Invokation: " + script);
				foreach (var node in invokation)
					node.Debug(1);
				Console.WriteLine();
			}

			return EtcScriptLib.Declaration.MatchesHeaderPattern(invokation, declaration.Terms);
		}

		private static bool DoesNotMatchDeclaration(String header, String script, bool debug = false)
		{
			return !MatchesDeclaration(header, script, debug);
		}

        [Test]
        public void basic_pattern()
        {
			Assert.IsTrue(MatchesDeclaration("a b c", "a b c"));
			Assert.IsTrue(DoesNotMatchDeclaration("a b c", "c b a"));
        }

		[Test]
		public void optional_terms()
		{
			Assert.IsTrue(MatchesDeclaration("a b ? c", "a b c"));
			Assert.IsTrue(MatchesDeclaration("a b ? c", "a c"));
			Assert.IsTrue(MatchesDeclaration("a b ? c", "a (b) c"));
			Assert.IsTrue(MatchesDeclaration("a (b)? c", "a (b) c"), "Optional argument");
			Assert.IsTrue(MatchesDeclaration("a ? b c", "b c"));
		}

		[Test]
		public void right_number_of_terms()
		{
			Assert.IsTrue(DoesNotMatchDeclaration("a b c", "a b c d", true));
			Assert.IsTrue(DoesNotMatchDeclaration("a b c", "a b", true));
		}

		[Test]
		public void none_or_many()
		{
			Assert.IsTrue(MatchesDeclaration("a b *", "a b b b b b b b b", true));
			Assert.IsTrue(MatchesDeclaration("a b *", "a"));
			Assert.IsTrue(MatchesDeclaration("a b * c", "a b b b b b b c"));
			Assert.IsTrue(DoesNotMatchDeclaration("a (b)* c", "a b b b b b c"), "b term consumes all following terms.");
		}

		[Test]
		public void match_operators()
		{
			Assert.IsTrue(MatchesDeclaration("a \"+\" b", "a + b", true));
			Assert.IsTrue(MatchesDeclaration("(a) \"+\" (b)", "foo + bar", true));
		}
    }

}