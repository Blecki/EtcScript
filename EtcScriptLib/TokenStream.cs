﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
	public class TokenStream : Iterator<Token>
	{
		private Iterator<int> source;
		private CodeLocation location;
		private Token? next_token;

		private OperatorSettings operators;
		private String delimeters = "!$%^&*()-=+|/<> \t\r\n\".";

		private void advance_source()
		{
			if (source.AtEnd()) return;

			var r = source.Next();
			source.Advance();
			location.Character += 1;

			if (r == '\r' || r == '\n')
			{
				location.Line += 1;
				location.Character = 0;
			}
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

		private bool isValidIdentifierBody(int c)
		{
			return c == '_' || isAlpha(c) || isDigit(c);
		}

		private bool isValidIdentifierStart(int c)
		{
			return c == '_' || isAlpha(c);
		}

		public TokenStream(Iterator<int> Source, OperatorSettings operators)
		{
			this.source = Source;
			next_token = ParseNextToken();
			this.operators = operators;
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
			while ((c == ' ' || c == '\r') && !source.AtEnd())
			{
				advance_source();
				if (!source.AtEnd()) c = source.Next();
			}

			if (c == '#')
				while (!source.AtEnd() && source.Next() != '\n') advance_source();

			if (source.AtEnd()) return null;
			else c = source.Next();

			var tokenStart = location;

			if (c == '\n') { advance_source(); return Token.Create(TokenType.NewLine, "\\n", tokenStart); }
			if (c == '\t') { advance_source(); return Token.Create(TokenType.Tab, "\\t", tokenStart); }
			if (c == '(') { advance_source(); return Token.Create(TokenType.OpenParen, "(", tokenStart); }
			if (c == ')') { advance_source(); return Token.Create(TokenType.CloseParen, ")", tokenStart); }
			if (c == '[') { advance_source(); return Token.Create(TokenType.OpenBracket, "[", tokenStart); }
			if (c == ']') { advance_source(); return Token.Create(TokenType.CloseBracket, "]", tokenStart); }
			if (c == '.') { advance_source(); return Token.Create(TokenType.Dot, ".", tokenStart); }
			if (c == ';') { advance_source(); return Token.Create(TokenType.Semicolon, ";", tokenStart); }
			if (c == '\"')
			{
				var literal = "";
				advance_source();
				c = source.Next();
				while (c != '\"' && !source.AtEnd())
				{
					literal += (char)c;
					advance_source();
					c = source.Next();
				}
				advance_source();
				return Token.Create(TokenType.String, literal, tokenStart);
			}

			if (isValidIdentifierStart(c))
			{

				var identifier = "";
				while (isValidIdentifierBody(c) && !source.AtEnd())
				{
					identifier += (char)c;
					advance_source();
					if (!source.AtEnd()) c = source.Next();
				}
				return Token.Create(TokenType.Identifier, identifier, tokenStart);
			}

			var parsedMinus = false;
			if (c == '-')
			{
				parsedMinus = true;
				advance_source();
				var number = ParseNumber();
				if (number.Length != 0) return Token.Create(TokenType.Number, "-" + number, tokenStart);
			}
			else if (isDigit(c))
				return Token.Create(TokenType.Number, ParseNumber(), tokenStart);

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
						return Token.Create(TokenType.Operator, tempOp, tokenStart);
					}
				}

				opSoFar = tempOp;
			}
		}

		public Token Next()
		{
			return next_token.Value;
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