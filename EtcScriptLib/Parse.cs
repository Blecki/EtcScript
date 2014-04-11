using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class Parse
    {
		private static bool IsEndOfStatement(Iterator<Token> Stream)
		{
			if (Stream.AtEnd()) return true;
			return Stream.Next().Type == TokenType.Semicolon;
		}

		private static Ast.Let ParseLetStatement(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			var start = Stream.Next();

			if (Stream.AtEnd() || Stream.Next().Type != TokenType.Identifier || Stream.Next().Value.ToUpper() != "LET")
				throw new CompileError("Impossible error: Let parse entered, no let found.", Stream);

			Stream.Advance();
			
			var LHS = ParseTerm(Stream, Context);

			if (LHS is Ast.MemberAccess)
				if ((LHS as Ast.MemberAccess).DefaultValue != null)
					throw new CompileError("Default value illegal on dynamic set", (LHS as Ast.MemberAccess).DefaultValue.Source);

			if (Stream.Next().Type != TokenType.Operator || Stream.Next().Value != "=")
				throw new CompileError("Expected '='", Stream);
			Stream.Advance();

			var RHS = ParseExpression(Stream, Context, TokenType.Semicolon);

			if (!IsEndOfStatement(Stream)) throw new CompileError("Expected ;", Stream);
			Stream.Advance();
			return new Ast.Let(start, LHS, RHS);
		}

		private static Ast.LocalDeclaration ParseLocalDeclaration(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			var start = Stream.Next();

			Stream.Advance();

			if (Stream.Next().Type != TokenType.Identifier)
				throw new CompileError("Expected identifier", Stream);

			var r = new Ast.LocalDeclaration(start);
			r.Name = Stream.Next().Value;
			Stream.Advance();

			if (Stream.Next().Type == TokenType.Semicolon)
			{
				Stream.Advance();
				return r;
			}
			else if (Stream.Next().Type == TokenType.Operator && Stream.Next().Value == "=")
			{
				Stream.Advance();
				r.Value = ParseExpression(Stream, Context, TokenType.Semicolon);
				if (!IsEndOfStatement(Stream)) throw new CompileError("Expected ;", Stream);
				Stream.Advance();
				return r;
			}
			throw new CompileError("Expected ; or =", Stream);
		}

		private static Ast.Return ParseReturnStatement(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			var start = Stream.Next();

			if (Stream.AtEnd() || Stream.Next().Type != TokenType.Identifier || Stream.Next().Value.ToUpper() != "RETURN")
				throw new CompileError("Impossible error: Return parse entered, no return found.", Stream);

			Stream.Advance();

			var r = new Ast.Return(start);
			if (Stream.Next().Type != TokenType.Semicolon)
				r.Value = ParseExpression(Stream, Context, TokenType.Semicolon);

			if (Stream.Next().Type != TokenType.Semicolon) throw new CompileError("Expected ;", Stream);
			Stream.Advance();
			return r;
		}

		private static Ast.If ParseIfStatement(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			if (Stream.AtEnd() || Stream.Next().Type != TokenType.Identifier || Stream.Next().Value.ToUpper() != "IF")
				throw new CompileError("Impossible error: If parse entered, no if found.", Stream);
			var r = new Ast.If(Stream.Next());

			Stream.Advance();  //Skip 'if'
			r.Header = ParseExpression(Stream, Context, TokenType.OpenBrace);

			if (Stream.Next().Type != TokenType.OpenBrace) throw new CompileError("Expected {", Stream);
			
			r.ThenBlock = ParseBlock(Stream, Context);

			if (Stream.Next().Type == TokenType.Identifier && Stream.Next().Value.ToUpper() == "ELSE")
			{
				Stream.Advance();
				if (Stream.Next().Type == TokenType.OpenBrace)
					r.ElseBlock = ParseBlock(Stream, Context);
				else if (Stream.Next().Type == TokenType.Identifier && Stream.Next().Value.ToUpper() == "IF")
					r.ElseBlock = ParseIfStatement(Stream, Context);
			}

			return r;
		}

		private static Ast.Statement ParseStatement(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			Ast.Statement r = null;

			var firstToken = Stream.Next().Value.ToUpper();

			if (firstToken == "LET")
			{
				r = ParseLetStatement(Stream, Context);
			}
			else if (firstToken == "IF")
			{
				r = ParseIfStatement(Stream, Context);
			}
			else if (firstToken == "RETURN")
			{
				r = ParseReturnStatement(Stream, Context);
			}
			else if (firstToken == "VAR" || firstToken == "VARIABLE")
			{
				r = ParseLocalDeclaration(Stream, Context);
			}
			else if (Stream.Next().Type == TokenType.Colon)
			{
				Stream.Advance();
				var parameters = ParseDynamicInvokation(Stream, Context);
				r = new Ast.CompatibleCall(parameters[0].Source, new Ast.AssembleList(new Token(), parameters));
				if (Stream.Next().Type != TokenType.Semicolon) throw new CompileError("Expected ;", Stream);
				Stream.Advance();
			}
			else
			{
				//If it's not any special statement, it must be a function call.
				var parameters = ParseStaticInvokationStatement(Stream, Context);
				var control = Context.FindControl(parameters);
				if (control != null)
				{
					Ast.Node childBlock = null;
					if (control.BlockType == ControlBlockType.RequiredBlock)
					{
						if (Stream.Next().Type == TokenType.OpenBrace)
							childBlock = ParseBlock(Stream, Context);
						else
							throw new CompileError("Expected {", Stream);
					}
					else
					{
						if (Stream.Next().Type != TokenType.Semicolon) throw new CompileError("Expected ;", Stream);
						Stream.Advance();
					}
					r = new Ast.ControlInvokation(parameters[0].Source, control, parameters, childBlock);
				}
				else
				{
					r = new Ast.StaticInvokation(parameters[0].Source, parameters);
					if (Stream.Next().Type != TokenType.Semicolon) throw new CompileError("Expected ;", Stream);
					Stream.Advance();
				}
			}
			
			return r;
		}

		private static Ast.Node ParseOptionalDot(
			Iterator<Token> Stream,
			Ast.Node LHS,
			ParseContext Context)
		{
			if (Stream.AtEnd()) return LHS;

			if (Stream.Next().Type == TokenType.Dot)
			{
				Stream.Advance();
				if (Stream.Next().Type != TokenType.Identifier)
					throw new CompileError("Dot operator must be followed by identifier", Stream);
				var RHS = Stream.Next().Value;
				var MA = new Ast.MemberAccess(LHS.Source);
				MA.Object = LHS;
				MA.Name = RHS;
				Stream.Advance();

				return ParseOptionalDot(Stream, MA, Context);
			}
			else if (Stream.Next().Type == TokenType.QuestionMark)
			{
				Stream.Advance();
				if (Stream.Next().Type != TokenType.Identifier)
					throw new CompileError("? operator must be followed by identifier", Stream);
				var RHS = Stream.Next().Value;
				var MA = new Ast.MemberAccess(LHS.Source);
				MA.Object = LHS;
				MA.Name = RHS;
				MA.IsDynamicAccess = true;
				Stream.Advance();

				if (!Stream.AtEnd() && Stream.Next().Type == TokenType.Colon)
				{
					Stream.Advance();
					var defaultValue = ParseTerm(Stream, Context);
					MA.DefaultValue = defaultValue;
				}

				return ParseOptionalDot(Stream, MA, Context);
			}
			else
				return LHS;
		}		

		private static Ast.Node ParseTerm(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			if (IsEndOfStatement(Stream)) throw new CompileError("Expected argument", Stream);
			Ast.Node r = null;
			if (Stream.Next().Type == TokenType.OpenParen)
			{
				Stream.Advance();
				r = ParseExpression(Stream, Context, TokenType.CloseParen);
				Stream.Advance();
			}
			else if (Stream.Next().Type == TokenType.OpenBracket)
			{
				var parameters = ParseStaticInvokation(Stream, Context);
				r = new Ast.StaticInvokation(parameters[0].Source, parameters);
			}
			else if (Stream.Next().Type == TokenType.Colon)
			{
				Stream.Advance();
				var parameters = ParseDynamicInvokation(Stream, Context);
				r = new Ast.CompatibleCall(parameters[0].Source, new Ast.AssembleList(new Token(), parameters));
			}
			else if (Stream.Next().Type == TokenType.Identifier ||
				Stream.Next().Type == TokenType.Number ||
				Stream.Next().Type == TokenType.String)
			{
				r = new Ast.Identifier(Stream.Next());
				(r as Ast.Identifier).Name = Stream.Next();
				Stream.Advance();
			}
			else if (Stream.Next().Type == TokenType.Operator)
			{
				r = new Ast.Identifier(Stream.Next());
				(r as Ast.Identifier).Name = Stream.Next();
				Stream.Advance();
			}
			else
				throw new CompileError("Illegal token in argument list", Stream.Next());

			r = ParseOptionalDot(Stream, r, Context);

			return r;
		}

		public static List<Ast.Node> ParseStaticInvokation(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			if (Stream.Next().Type != TokenType.OpenBracket) throw new CompileError("Expected [", Stream);
			Stream.Advance();

			 var parameters = new List<Ast.Node>();
			 while (true)
			 {
				 if (IsEndOfStatement(Stream)) throw new CompileError("Expected ]", Stream);
				 if (Stream.Next().Type == TokenType.CloseBracket)
				 {
					 Stream.Advance();
					 return parameters;
				 }
				 parameters.Add(ParseTerm(Stream, Context));
			 }
		}

		public static List<Ast.Node> ParseStaticInvokationStatement(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			var parameters = new List<Ast.Node>();
			while (true)
			{
				if (IsEndOfStatement(Stream) || Stream.Next().Type == TokenType.OpenBrace)
				{
					//Why don't we skip the semi-colon? This is called when parsing control macros where it might be optional.
					return parameters;
				}
				parameters.Add(ParseTerm(Stream, Context));
			}
		}

		private static List<Ast.Node> ParseDynamicInvokation(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			if (Stream.Next().Type != TokenType.OpenBracket) throw new CompileError("Expected [", Stream);
			Stream.Advance();
			var parameters = new List<Ast.Node>();
			while (true)
			{
				if (IsEndOfStatement(Stream)) throw new CompileError("Unexpected end of statement", Stream);
				if (Stream.Next().Type == TokenType.CloseBracket)
				{
					Stream.Advance();
					return parameters;
				}
				else parameters.Add(ParseTerm(Stream, Context));
			}
		}


		//Implements http://en.wikipedia.org/wiki/Operator-precedence_parser
		private static Ast.Node ParseExpression(
			Ast.Node lhs,
			Iterator<Token> state, 
			ParseContext operators,
			int minimum_precedence,
			TokenType terminal)
		{
			while (true)
			{
				if (state.AtEnd() && terminal != TokenType.NewLine)
					throw new CompileError("Unexpected end of line in expression", lhs.Source);
				if (state.AtEnd() && terminal == TokenType.NewLine) return lhs;
				if (state.Next().Type == terminal) return lhs;
				if (state.Next().Type != TokenType.Operator) throw new CompileError("Expected operator", state.Next());

				var precedence = operators.FindPrecedence(state.Next().Value);
				if (precedence < minimum_precedence) return lhs;

				var op = state.Next();
				state.Advance();
				var rhs = ParseTerm(state, operators);

				while (true)
				{
					if (state.AtEnd()) break;
					if (state.Next().Type == TokenType.Operator)
					{
						var next_precedence = operators.FindPrecedence(state.Next().Value);
						if (next_precedence > precedence)
							rhs = ParseExpression(rhs, state, operators, next_precedence, terminal);
						else
							break;
					}
					else
						break;
				}

				lhs = new Ast.BinaryOperator(lhs.Source, operators.FindOperator(op.Value).Value.instruction, lhs, rhs);
			}
		}

		private static Ast.Node ParseExpression(
			Iterator<Token> Stream,
			ParseContext Context,
			TokenType terminal)
		{
			return ParseExpression(ParseTerm(Stream, Context), Stream, Context, 0, terminal);
		}

		private static Ast.BlockStatement ParseBlock(
			Iterator<Token> Stream,
			ParseContext Context)
		{
			var r = new Ast.BlockStatement(Stream.Next());
			Stream.Advance(); //Skip the opening brace.

			while (Stream.Next().Type != TokenType.CloseBrace)
				r.Statements.Add(ParseStatement(Stream, Context));

			Stream.Advance(); //Skip the closing brace.
			return r;
		}
		
		internal static DeclarationTerm ParseDeclarationTerm(Iterator<Token> Stream)
		{
			DeclarationTerm r = new DeclarationTerm();
			var start = Stream.Next();

			if (Stream.Next().Type == TokenType.Identifier)
			{
				r = new DeclarationTerm
				{
					Name = Stream.Next().Value.ToUpper(),
					Type = DeclarationTermType.Keyword,
					RepititionType = DeclarationTermRepititionType.Once
				};
				Stream.Advance();
			}
			else if (Stream.Next().Type == TokenType.OpenParen)
			{
				Stream.Advance();
				if (Stream.Next().Type != TokenType.Identifier)
					throw new CompileError("Expected identifier", start);
				r = new DeclarationTerm
				{
					Name = Stream.Next().Value,
					Type = DeclarationTermType.Term,
					RepititionType = DeclarationTermRepititionType.Once
				};
				Stream.Advance();
				if (Stream.Next().Type != TokenType.CloseParen)
					throw new CompileError("Expected )", start);
				Stream.Advance();
			}
			else if (Stream.Next().Type == TokenType.String)
			{
				r = new DeclarationTerm
				{
					Name = Stream.Next().Value.ToUpper(),
					Type = DeclarationTermType.Keyword,
					RepititionType = DeclarationTermRepititionType.Once
				};
				Stream.Advance();
			}
			else
				throw new CompileError("Illegal token in declaration header", start);

			if (!Stream.AtEnd())
			{
				if (Stream.Next().Type == TokenType.Operator || Stream.Next().Type == TokenType.QuestionMark)
				{
					var marker = Stream.Next();
					var repititionMarker = Stream.Next().Value;
					Stream.Advance();
					if (repititionMarker == "?")
						r.RepititionType = DeclarationTermRepititionType.Optional;
					else if (repititionMarker == "+")
						r.RepititionType = DeclarationTermRepititionType.OneOrMany;
					else if (repititionMarker == "*")
						r.RepititionType = DeclarationTermRepititionType.NoneOrMany;
					else
						throw new CompileError("Unrecognized repitition marker on declaration term", marker);
				}
			}

			return r;
		}

		internal enum DeclarationHeaderTerminatorType
		{
			StreamEnd,
			OpenBrace,
			OpenBraceOrWhen
		}

		internal static List<DeclarationTerm> ParseMacroDeclarationHeader(Iterator<Token> Stream, 
			DeclarationHeaderTerminatorType TerminatorType)
		{
			var r = new List<DeclarationTerm>();
			while (true)
			{
				if (TerminatorType == DeclarationHeaderTerminatorType.StreamEnd && Stream.AtEnd()) return r;
				else if (TerminatorType == DeclarationHeaderTerminatorType.OpenBrace && Stream.Next().Type == TokenType.OpenBrace) return r;
				else if (TerminatorType == DeclarationHeaderTerminatorType.OpenBraceOrWhen)
				{
					if (Stream.Next().Type == TokenType.OpenBrace) return r;
					else if (Stream.Next().Value.ToUpper() == "WHEN") return r;
				}
				r.Add(ParseDeclarationTerm(Stream));
			}
		}

		internal static Declaration ParseMacroDeclaration(Iterator<Token> Stream, ParseContext Context)
		{
			if (Stream.AtEnd()) throw new CompileError("Impossible error: ParseDeclaration entered at end of stream.", Stream);

			try
			{
				var r = new Declaration();

				if (Stream.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", Stream.Next());

				r.UsageSpecifier = Stream.Next().Value;
				Stream.Advance();

				r.Terms = ParseMacroDeclarationHeader(Stream, DeclarationHeaderTerminatorType.OpenBrace);

				if (!Stream.AtEnd() && Stream.Next().Type == TokenType.OpenBrace)
				{
					Context.PushNewScope();
					Context.ActiveScope.ChangeToFunctionType();
					CreateParameterDescriptors(Context.ActiveScope, r.Terms);
					r.Body = new LambdaBlock(ParseBlock(Stream, Context));
					r.DeclarationScope = Context.ActiveScope;
					Context.PopScope();
				}
				else
					throw new CompileError("Expected block", Stream);

				return r;
			}
			catch (CompileError ce)
			{
				throw ce;
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + e.StackTrace, Stream);
			}
		}

		// Given a list of terms, create the variable objects to represent the parameters of the declaration
		private static void CreateParameterDescriptors(ParseScope Scope, List<DeclarationTerm> Terms)
		{
			int parameterIndex = -3;
			for (int i = Terms.Count - 1; i >= 0; --i)
			{
				if (Terms[i].Type == DeclarationTermType.Term)
				{
					var variable = new Variable
					{
						Name = Terms[i].Name,
						Offset = parameterIndex
					};
					--parameterIndex;
					Scope.Variables.Add(variable);
				}
			}
		}

		internal static Declaration ParseRuleDeclaration(Iterator<Token> Stream, ParseContext Context)
		{
			if (Stream.AtEnd()) throw new CompileError("Impossible error: ParseRuleDeclaration entered at end of stream.", Stream);

			try
			{
				var r = new Declaration();

				if (Stream.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", Stream.Next());

				r.UsageSpecifier = Stream.Next().Value;
				Stream.Advance();

				r.Terms = ParseMacroDeclarationHeader(Stream, DeclarationHeaderTerminatorType.OpenBraceOrWhen);

				Context.PushNewScope();
				Context.ActiveScope.ChangeToFunctionType();
				CreateParameterDescriptors(Context.ActiveScope, r.Terms);

				if (Stream.Next().Value.ToUpper() == "WHEN")
				{
					Stream.Advance();
					r.WhenClause = new WhenClause(ParseExpression(Stream, Context, TokenType.OpenBrace));
				}

				if (Stream.Next().Type == TokenType.OpenBrace)
				{
					r.Body = new LambdaBlock(ParseBlock(Stream, Context));
					r.DeclarationScope = Context.ActiveScope;
					Context.PopScope();
				}
				else
				{
					Context.PopScope();
					throw new CompileError("Expected block", Stream);
				}

				return r;
			}
			catch (CompileError ce)
			{
				throw ce;
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + e.StackTrace, Stream);
			}
		}

		internal static String ParseVariableDeclaration(Iterator<Token> Stream, ParseContext Context)
		{
			if (Stream.AtEnd()) throw new CompileError("Impossible error: ParseVariableDeclaration entered at end of stream.", Stream);

			try
			{
				if (Stream.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", Stream.Next());
				Stream.Advance();
				if (Stream.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", Stream.Next());
				var r = Stream.Next().Value;
				Stream.Advance();
				if (Stream.Next().Type != TokenType.Semicolon) throw new CompileError("Expected ;", Stream.Next());
				Stream.Advance();
				return r;
			}
			catch (CompileError ce)
			{
				throw ce;
			}
			catch (Exception e)
			{
				throw new CompileError(e.Message + e.StackTrace, Stream);
			}
		}


		public static List<Declaration> Build(
			Iterator<Token> Stream, 
			ParseContext Context,
			Func<String,ErrorStrategy> OnError)
        {
			var r = new List<Declaration>();
			while (!Stream.AtEnd())
			{
				try
				{
					if (Stream.Next().Type != TokenType.Identifier) throw new CompileError("Expected identifier", Stream);
					if (Stream.Next().Value.ToUpper() == "MACRO")
					{
						var declaration = ParseMacroDeclaration(Stream, Context);
						Context.ActiveScope.Macros.Add(declaration);
						Context.PendingEmission.Add(declaration);
					}
					else if (Stream.Next().Value.ToUpper() == "TEST")
					{
						var declaration = ParseMacroDeclaration(Stream, Context);
						r.Add(declaration);
						Context.PendingEmission.Add(declaration);
					}
					else if (Stream.Next().Value.ToUpper() == "RULE")
					{
						var declaration = ParseRuleDeclaration(Stream, Context);
						var rulebook = Context.Rules.FindMatchingRulebook(declaration.Terms);
						if (rulebook == null)
						{
							rulebook = new Rulebook { DeclarationTerms = declaration.Terms };
							Context.Rules.Rulebooks.Add(rulebook);
						}
						rulebook.Rules.Add(declaration);
						Context.PendingEmission.Add(declaration);
					}
					else
						throw new CompileError("Unknown declaration type", Stream);
				}
				catch (Exception e)
				{
					if (OnError(e.Message) == ErrorStrategy.Abort) return r;
					Stream.Advance(); //Prevent an error from causing an infinite loop
				}
			}
			return r;
		}
    }
}
