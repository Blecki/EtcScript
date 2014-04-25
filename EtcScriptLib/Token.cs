using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public struct CodeLocation
    {
        public int Line;
        public int Character;

		public override string ToString()
		{
			return Line.ToString() + ":" + Character;
		}
    }

	public enum TokenType
	{
		Unknown,

		Identifier,
		Operator,
		OpenParen,
		CloseParen,
		OpenBracket,
		CloseBracket,
		OpenBrace,
		CloseBrace,
		Dot,
		Semicolon,
		QuestionMark,
		Colon,
		At,
		Dollar,

		String,
		Number,

		EndOfFile,
		Tab,
		NewLine,

		ComplexStringPart,
		ComplexStringQuote, //Only produced as part of parsing a complex string.
	}

    public struct Token
    {
        public TokenType Type;
        public String Value;
        public CodeLocation Location;

        public static Token Create(TokenType Type, String Value, CodeLocation Location)
        {
            return new Token{ Type = Type,
                Value = Value,
                Location = Location };
        }

        public override string ToString()
        {
            return "[" + Type + ": " + Value + " loc: " + Location.ToString() + "]";
        }
    }
}
