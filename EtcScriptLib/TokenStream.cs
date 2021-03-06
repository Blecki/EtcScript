﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum TokenStreamState
	{
		Normal,
		ComplexString
	}

	public class TokenStream : Iterator<Token>
	{
		private StringIterator source;
		private CodeLocation location;
		private Token? next_token;
		private Stack<TokenStreamState> StateStack = new Stack<TokenStreamState>();
		public LoadedFile CurrentFile = null;

		private ParseContext operators;
		private String delimeters = "()[]{} \t\r\n.;:@?$";

		private void advance_source()
		{
			if (source.AtEnd()) return;

			var r = source.Next();
			source.Advance();
			location.Character += 1;

			if (r == '\n')
			{
				location.Line += 1;
				location.Character = 0;
			}

			location.Index = source.place;
		}

		private bool isDelimeter(int c)
		{
			return delimeters.Contains((char)c);
		}

		private bool isAlpha(int c)
		{
			return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
		}

		private bool isDigit(int c)
		{
			return c >= '0' && c <= '9';
		}

		private bool isValidIdentifierStart(int c)
		{
			return c == '_' || isAlpha(c);
		}

		public TokenStream(StringIterator Source, ParseContext operators)
		{
			this.source = Source;
			this.operators = operators;
			StateStack.Push(TokenStreamState.Normal);
			next_token = ParseNextToken();
		}

		public void PushState(TokenStreamState State)
		{
			StateStack.Push(State);
		}

		public void PopState()
		{
			StateStack.Pop();
		}

		private String ParseNumber()
		{
			var number = "";
			bool foundDot = false;
			var c = source.Next();
			while ((isDigit(c) || c == '.') && !source.AtEnd())
			{
				if (c == '.')
				{
					if (foundDot) return number;
					foundDot = true;
				}

				number += (char)c;
				advance_source();
				if (!source.AtEnd()) c = source.Next();
			}

			return number;
		}

		private Token? ParseNextToken()
		{
			var c = source.Next();

			var currentState = StateStack.Peek();

			if (currentState == TokenStreamState.Normal)
			{
				#region Normal mode tokenizing
				while ((c == ' ' || c == '\r' || c == '\n' || c == '\t' || c == '#') && !source.AtEnd())
				{
					if (c == '#') while (!source.AtEnd() && source.Next() != '\n') advance_source();
					advance_source();
					if (!source.AtEnd()) c = source.Next();
				}

				if (source.AtEnd()) return null;
				else c = source.Next();

				var tokenStart = location;
				tokenStart.EndIndex = source.place;

				//if (c == '\n') { advance_source(); return Token.Create(TokenType.NewLine, "\\n", tokenStart); }
				//if (c == '\t') { advance_source(); return Token.Create(TokenType.Tab, "\\t", tokenStart); }
				if (c == '(') { advance_source(); return Token.Create(TokenType.OpenParen, "(", tokenStart); }
				if (c == ')') { advance_source(); return Token.Create(TokenType.CloseParen, ")", tokenStart); }
				if (c == '[') { advance_source(); return Token.Create(TokenType.OpenBracket, "[", tokenStart); }
				if (c == ']') { advance_source(); return Token.Create(TokenType.CloseBracket, "]", tokenStart); }
				if (c == '{') { advance_source(); return Token.Create(TokenType.OpenBrace, "{", tokenStart); }
				if (c == '}') { advance_source(); return Token.Create(TokenType.CloseBrace, "}", tokenStart); }
				if (c == '.') { advance_source(); return Token.Create(TokenType.Dot, ".", tokenStart); }
				if (c == ';') { advance_source(); return Token.Create(TokenType.Semicolon, ";", tokenStart); }
				if (c == '?') { advance_source(); return Token.Create(TokenType.QuestionMark, "?", tokenStart); }
				if (c == ':') { advance_source(); return Token.Create(TokenType.Colon, ":", tokenStart); }
				if (c == '@') { advance_source(); return Token.Create(TokenType.At, "@", tokenStart); }
				if (c == '$') { advance_source(); return Token.Create(TokenType.Dollar, "$", tokenStart); }
				if (c == '\"')
				{
					advance_source();
					var literal = TokenizeStringLiteral(false);
					advance_source();
					tokenStart.EndIndex = source.place;
					return Token.Create(TokenType.String, literal, tokenStart);
				}

				if (isValidIdentifierStart(c))
				{

					var identifier = "";
					while (!isDelimeter(c) && !source.AtEnd())
					{
						identifier += (char)c;
						advance_source();
						if (!source.AtEnd()) c = source.Next();
					}
					tokenStart.EndIndex = source.place;
					return Token.Create(TokenType.Identifier, identifier.ToUpper(), tokenStart);
				}

				var parsedMinus = false;
				if (c == '-')
				{
					parsedMinus = true;
					advance_source();
					var number = ParseNumber();
					tokenStart.EndIndex = source.place;
					if (number.Length != 0) return Token.Create(TokenType.Number, "-" + number, tokenStart);
				}
				else if (isDigit(c))
				{
					var number = ParseNumber();
					tokenStart.EndIndex = source.place;
					return Token.Create(TokenType.Number, number, tokenStart);
				}

				var opSoFar = new String((char)c, 1);
				while (true)
				{
					if (parsedMinus) parsedMinus = false; //If we read a -, but not a number token, source is already pointing at
					else advance_source();					//the next character

					var tempOp = opSoFar + (source.AtEnd() ? "" : new String((char)source.Next(), 1));

					var possibleMatches = operators.operatorStrings.Count((s) => { return s.StartsWith(tempOp); });

					if (possibleMatches == 0)
					{
						return Token.Create(TokenType.Operator, opSoFar, tokenStart);
					}
					else if (possibleMatches == 1)
					{
						var exactMatches = operators.operatorStrings.Count((s) => { return s == tempOp; });
						if (exactMatches == 1)
						{
							advance_source();
							tokenStart.EndIndex = source.place;
							return Token.Create(TokenType.Operator, tempOp, tokenStart);
						}
					}

					opSoFar = tempOp;
				}
				#endregion
			}
			else if (currentState == TokenStreamState.ComplexString)
			{
				//Despite being 'complex', ComplexString is far simpler to tokenize. It's any sequence of characters except
				//	" or (.
				var tokenStart = location;

				if (c == '[') { advance_source(); tokenStart.EndIndex = source.place;  return Token.Create(TokenType.OpenBracket, "[", tokenStart); }
				if (c == '"') { advance_source(); tokenStart.EndIndex = source.place;  return Token.Create(TokenType.ComplexStringQuote, "\"", tokenStart); }
				
				var text = TokenizeStringLiteral(true);
				tokenStart.EndIndex = source.place;
				return Token.Create(TokenType.ComplexStringPart, text, tokenStart);
			}
			else
				throw new InvalidProgramException();
		}

		private String TokenizeStringLiteral(bool Complex)
		{
			var literal = "";
			var c = source.Next();
			while (!source.AtEnd() && c != '"')
			{
				if (c == '[' && Complex) return literal;

				if (c == '\\')
				{
					advance_source();
					c = source.Next();

					if (c == 'n')
						literal += '\n';
					else if (c == 'x')
					{
						var hex = "";
						advance_source();
						hex += (char)source.Next();
						advance_source();
						hex += (char)source.Next();
						literal += (char)(Convert.ToInt32(hex, 16));
					}
					else
						literal += (char)c;

					advance_source();
					if (!source.AtEnd()) c = source.Next();
				}
				else
				{
					literal += (char)c;
					advance_source();
					if (!source.AtEnd()) c = source.Next();
				}
			}
			return literal;
		}

		public Token Next()
		{
			if (next_token.HasValue) return next_token.Value;
			throw new CompileError("Unexpected end of token stream", this);
		}

		public void Advance()
		{
			next_token = null;
			if (!source.AtEnd()) next_token = ParseNextToken();
		}

		public bool AtEnd()
		{
			return !next_token.HasValue;
		}
	}
}
