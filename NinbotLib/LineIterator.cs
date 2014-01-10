using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
	internal class LineIterator : Iterator<Line>
	{
		public Iterator<Token> stream;
		private Line next;
		private int foundEnd = 2;

		private LineIterator() { }

		internal static LineIterator Create(Iterator<Token> stream)
		{
			var r = new LineIterator { stream = stream };
			r.Advance();
			return r;
		}

		public Line Next()
		{
			return next;
		}

		public void Advance()
		{
			next = null;

			if (stream.AtEnd()) return;
			next = new Line(0);

			while (!stream.AtEnd())
			{
				var token = stream.Next();
				if (token.Type == TokenType.NewLine)
				{
					if (next.Tokens.Count == 0) next.IndentionLevel = 0; //Collapse blank lines
					stream.Advance();
					break;
				}
				else if (token.Type == TokenType.Tab && next.Tokens.Count == 0)
					next.IndentionLevel += 1;
				else
					next.Tokens.Add(token);

				stream.Advance();
			}

		}

		public bool AtEnd()
		{
			return next == null;
		}
	}
}
