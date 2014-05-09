using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum TokenState
	{
		Start,
	}

	public struct SyntaxLexerState
	{
		public int FoldDepth;
		public TokenState TokenState;
	}

	//public class SyntaxLexer
	//{
	//    public Dictionary<int, List<SyntaxLexerState>> LineStates = new Dictionary<int, List<SyntaxLexerState>>();


	public class SyntaxLex
	{
		public enum TokenStyle
		{
			Error,
			Keyword,
			Clause,
			Operator,
			String,
			ComplexStringPart,
			Number,
			Basic
		}

		TokenStream Stream;
		Action<Token, TokenStyle, int> StyleToken;
		Action<Token> MarkFold;
		ParseContext Context;
		int FoldLevel = 0;

		public SyntaxLex(ParseContext Context, 
			Action<Token, TokenStyle, int> StyleToken,
			Action<Token> MarkFold)
		{
			this.Context = Context;
			this.StyleToken = StyleToken;
			this.MarkFold = MarkFold;
		}

		private void Style(TokenStyle Style)
		{
			StyleToken(Stream.Next(), Style, FoldLevel);
			Stream.Advance();
		}

		private void StartFold()
		{
			MarkFold(Stream.Next());
			++FoldLevel;
		}

		private void EndFold()
		{
			--FoldLevel;
		}

		private void Expect(TokenType Type)
		{
			if (Stream.Next().Type != Type)
				Style(TokenStyle.Error);
			else
			{
				if (Type == TokenType.Operator)
					Style(TokenStyle.Operator);
				else
					Style(TokenStyle.Basic);
			}
		}

		private void Expect(TokenType Type, Predicate<Token> Func)
		{
			if (Stream.Next().Type != Type || !Func(Stream.Next()))
				Style(TokenStyle.Error);
			else
			{
				if (Type == TokenType.Operator)
					Style(TokenStyle.Operator);
				else
					Style(TokenStyle.Basic);
			}
		}

		private void ExpectEquals()
		{
			Expect(TokenType.Operator, t => t.Value == "=");
		}

		private void ParseLetStatement()
		{
			Style(TokenStyle.Keyword);
			ParseTerm();
			ExpectEquals();
			ParseExpression(TokenType.Semicolon);
			Expect(TokenType.Semicolon);
		}

		private void ParseLocalDeclaration()
		{
			Style(TokenStyle.Keyword);
			Expect(TokenType.Identifier);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}
			if (Stream.Next().Type == TokenType.Operator)
			{
				ExpectEquals();
				ParseExpression(TokenType.Semicolon);
			}
			Expect(TokenType.Semicolon);
		}

		private void ParseReturnStatement()
		{
			Style(TokenStyle.Keyword);
			if (Stream.Next().Type != TokenType.Semicolon)
				ParseExpression(TokenType.Semicolon);
			Expect(TokenType.Semicolon);
		}

		private void ParseIfStatement()
		{
			Style(TokenStyle.Keyword);
			ParseExpression(TokenType.OpenBrace);
			ParseBlock();
			if (Stream.Next().Value.ToUpper() == "ELSE")
			{
				Style(TokenStyle.Keyword);
				if (Stream.Next().Value.ToUpper() == "IF")
					ParseIfStatement();
				else
				{
					ParseBlock();
				}
			}
		}

		private void ParseStatement()
		{
			var firstToken = Stream.Next().Value.ToUpper();
			if (firstToken == "LET") ParseLetStatement();
			else if (firstToken == "IF") ParseIfStatement();
			else if (firstToken == "RETURN") ParseReturnStatement();
			else if (firstToken == "VAR" || firstToken == "VARIABLE") ParseLocalDeclaration();
			else
			{
				ParseStaticInvokationStatement();
				if (Stream.Next().Type == TokenType.OpenBrace)
				{
					ParseBlock();
				}
				else
					Expect(TokenType.Semicolon);
			}
		}

		private void ParseOptionalDot()
		{
			if (Stream.Next().Type == TokenType.Dot)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
				ParseOptionalDot();
			}
			else if (Stream.Next().Type == TokenType.At)
			{
				Style(TokenStyle.Operator);
				ParseTerm();
				ParseOptionalDot();
			}
			else if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
				ParseOptionalDot();
			}
		}

		private void ParseTerm()
		{
			if (Stream.Next().Type == TokenType.OpenParen)
			{
				Style(TokenStyle.Basic);
				ParseExpression(TokenType.CloseParen);
				Expect(TokenType.CloseParen);
			}
			else if (Stream.Next().Type == TokenType.OpenBracket)
				ParseStaticInvokation();
			else if (Stream.Next().Type == TokenType.Identifier)
				Style(TokenStyle.Basic);
			else if (Stream.Next().Type == TokenType.String)
				Style(TokenStyle.String);
			else if (Stream.Next().Type == TokenType.Number)
				Style(TokenStyle.Number);
			else if (Stream.Next().Type == TokenType.Operator)
				Style(TokenStyle.Operator);
			else if (Stream.Next().Type == TokenType.Dollar)
				ParseComplexString();
			else if (Stream.Next().Type == TokenType.OpenBrace)
			{
				Style(TokenStyle.Basic);
				while (Stream.Next().Type != TokenType.CloseBrace)
					ParseTerm();
				Expect(TokenType.CloseBrace);
			}
			else
				Style(TokenStyle.Error);

			ParseOptionalDot();
		}

		public void ParseComplexString()
		{
			Stream.PushState(TokenStreamState.ComplexString); //Enter complex string parsing mode
			Style(TokenStyle.Operator);
			Expect(TokenType.ComplexStringQuote);

			while (Stream.Next().Type != TokenType.ComplexStringQuote)
			{
				if (Stream.Next().Type == TokenType.ComplexStringPart)
					Style(TokenStyle.ComplexStringPart);
				else if (Stream.Next().Type == TokenType.OpenBracket)
				{
					Stream.PushState(TokenStreamState.Normal); //Make sure we are parsing normally for the
					Style(TokenStyle.Basic);
					ParseExpression(TokenType.CloseBracket);
					if (Stream.Next().Type != TokenType.CloseBracket)
						StyleToken(Stream.Next(), TokenStyle.Error, FoldLevel);
					else
						StyleToken(Stream.Next(), TokenStyle.Basic, FoldLevel);
					Stream.PopState();
					Stream.Advance();
				}
				else
					break;
			}

			Stream.PopState(); //Return to normal parsing mode
			Expect(TokenType.ComplexStringQuote);
		}

		public void ParseStaticInvokation()
		{
			Style(TokenStyle.Basic);
			while (Stream.Next().Type != TokenType.CloseBracket)
				ParseTerm();
			Expect(TokenType.CloseBracket);
		}

		public void ParseStaticInvokationStatement()
		{
			while (Stream.Next().Type != TokenType.Semicolon && Stream.Next().Type != TokenType.OpenBrace)
				ParseTerm();
		}

		private void ParseExpression(TokenType Terminal)
		{
			if (Terminal == TokenType.NewLine)
				ParseExpression((stream) =>
					{
						return stream.AtEnd() || stream.Next().Type == TokenType.NewLine;
					});
			else
				ParseExpression((stream) => { return stream.Next().Type == Terminal; });
		}

		private void ParseExpression(Predicate<TokenStream> IsTerminal)
		{
			if (Stream.Next().Value.ToUpper() == "LAMBDA")
				ParseMacroDeclaration();
			else if (Stream.Next().Value.ToUpper() == "NEW")
				ParseNew();
			else
			{
				ParseTerm();
				while (true)
				{
					if (IsTerminal(Stream)) return;
					Expect(TokenType.Operator);
					ParseTerm();
				}
			}
		}

		private void ParseNew()
		{
			Style(TokenStyle.Keyword);
			Expect(TokenType.Identifier);
			if (Stream.Next().Type == TokenType.OpenBrace)
				ParseInitializers();
		}

		private void ParseInitializers()
		{
			StartFold();
			Expect(TokenType.OpenBrace);
			while (Stream.Next().Type != TokenType.CloseBrace)
			{
				Expect(TokenType.Identifier, t => t.Value.ToUpper() == "LET");
				Expect(TokenType.Identifier);
				ExpectEquals();
				ParseExpression(TokenType.Semicolon);
				Expect(TokenType.Semicolon);
			}
			Expect(TokenType.CloseBrace);
			EndFold();
		}

		private void ParseBlock()
		{
			StartFold();
			Expect(TokenType.OpenBrace);
			while (Stream.Next().Type != TokenType.CloseBrace)
				ParseStatement();
			Expect(TokenType.CloseBrace);
			EndFold();
		}

		internal void ParseDeclarationTerm()
		{
			if (Stream.Next().Type == TokenType.Identifier)
				Style(TokenStyle.Basic);
			else if (Stream.Next().Type == TokenType.Operator)
				Style(TokenStyle.Operator);
			else if (Stream.Next().Type == TokenType.OpenParen)
			{
				Style(TokenStyle.Basic);
				Expect(TokenType.Identifier);
				if (Stream.Next().Type == TokenType.Colon)
				{
					Style(TokenStyle.Operator);
					Expect(TokenType.Identifier);
				}
				Expect(TokenType.CloseParen);
			}

			if (Stream.Next().Type == TokenType.QuestionMark)
				Style(TokenStyle.Operator);
		}

		internal enum DeclarationHeaderTerminatorType
		{
			StreamEnd,
			OpenBrace,
			OpenBraceOrWhen
		}

		internal void ParseMacroDeclarationHeader(DeclarationHeaderTerminatorType TerminatorType)
		{
			while (true)
			{
				if (TerminatorType == DeclarationHeaderTerminatorType.StreamEnd && Stream.AtEnd()) return;
				else if (Stream.Next().Type == TokenType.Colon) return;
				else if (TerminatorType == DeclarationHeaderTerminatorType.OpenBrace && Stream.Next().Type == TokenType.OpenBrace) return;
				else if (TerminatorType == DeclarationHeaderTerminatorType.OpenBraceOrWhen)
				{
					if (Stream.Next().Type == TokenType.OpenBrace) return;
					else if (Stream.Next().Value.ToUpper() == "WHEN") return;
					else if (Stream.Next().Value.ToUpper() == "WITH") return;
					else if (Stream.Next().Value.ToUpper() == "ORDER") return;
				}

				ParseDeclarationTerm();
			}
		}

		internal void ParseMacroDeclaration()
		{
			Style(TokenStyle.Keyword);
			ParseMacroDeclarationHeader(DeclarationHeaderTerminatorType.OpenBrace);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}
			ParseBlock();
		}

		internal void ParseTypeDeclaration()
		{
			StartFold();
			Style(TokenStyle.Keyword);
			Expect(TokenType.Identifier);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}
			Expect(TokenType.OpenBrace);
			while (Stream.Next().Type != TokenType.CloseBrace)
				ParseMemberDeclaration();

			Expect(TokenType.CloseBrace);
			EndFold();
		}

		internal void ParseRuleDeclaration()
		{
			Style(TokenStyle.Keyword);
			ParseMacroDeclarationHeader(DeclarationHeaderTerminatorType.OpenBraceOrWhen);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}
			if (Stream.Next().Value.ToUpper() == "WHEN")
			{
				Style(TokenStyle.Clause);
				ParseExpression((stream) =>
					{
						if (stream.Next().Type == TokenType.OpenBrace) return true;
						if (stream.Next().Type == TokenType.Identifier &&
							(stream.Next().Value.ToUpper() == "WITH" ||
							stream.Next().Value.ToUpper() == "ORDER")) return true;
						return false;
					});
			}

			if (Stream.Next().Value.ToUpper() == "ORDER")
			{
				Style(TokenStyle.Clause);
				Expect(TokenType.Identifier, t => t.Value.ToUpper() == "FIRST" || t.Value.ToUpper() == "LAST");
			}

			if (Stream.Next().Value.ToUpper() == "WITH")
			{
				Style(TokenStyle.Clause);
				Expect(TokenType.Identifier, t => t.Value.ToUpper() == "LOW" || t.Value.ToUpper() == "HIGH");
				Expect(TokenType.Identifier, t => t.Value.ToUpper() == "PRIORITY");
			}

			ParseBlock();
		}

		internal void ParseMemberDeclaration()
		{
			Expect(TokenType.Identifier, t => t.Value.ToUpper() == "VAR" || t.Value.ToUpper() == "VARIABLE");
			Expect(TokenType.Identifier);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}
			Expect(TokenType.Semicolon);
		}

		internal void ParseGlobalDeclaration()
		{
			Style(TokenStyle.Keyword);
			Expect(TokenType.Identifier);
			if (Stream.Next().Type == TokenType.Colon)
			{
				Style(TokenStyle.Operator);
				Expect(TokenType.Identifier);
			}

			if (Stream.Next().Value == "=")
			{
				Style(TokenStyle.Operator);
				ParseExpression(TokenType.Semicolon);
			}

			Expect(TokenType.Semicolon);
		}

		public void Style(TokenStream Stream)
		{
			this.Stream = Stream;
			this.FoldLevel = 1;

			while (!Stream.AtEnd())
			{
				if (Stream.Next().Value.ToUpper() == "MACRO"
					|| Stream.Next().Value.ToUpper() == "FUNCTION"
					|| Stream.Next().Value.ToUpper() == "TEST")
					ParseMacroDeclaration();
				else if (Stream.Next().Value.ToUpper() == "RULE")
					ParseRuleDeclaration();
				else if (Stream.Next().Value.ToUpper() == "TYPE")
					ParseTypeDeclaration();
				else if (Stream.Next().Value.ToUpper() == "GLOBAL")
					ParseGlobalDeclaration();
				else if (Stream.Next().Value.ToUpper() == "INCLUDE")
				{
					Style(TokenStyle.Keyword);
					Expect(TokenType.String);
				}
				else if (Stream.Next().Value.ToUpper() == "DEFAULT")
				{
					Style(TokenStyle.Keyword);
					Expect(TokenType.Identifier, t => t.Value.ToUpper() == "OF");
					ParseRuleDeclaration();
				}
				else
					Stream.Advance();
			}
		}
	}
}