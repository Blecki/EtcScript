using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	internal class BlockIterator : Iterator<Block>
	{
		internal Iterator<Line> state;
		internal Block next;

		internal static BlockIterator Create(Iterator<Line> state)
		{
			return new BlockIterator(state);
		}

		internal BlockIterator(Iterator<Line> state)
		{
			this.state = state;
			Advance();
		}

		public Block Next()
		{
			return next;
		}

		public void Advance()
		{
			next = null;

			while (!state.AtEnd() && state.Next().IsEmptyLine())
				state.Advance();

			if (!state.AtEnd()) next = FindBlock(state);
		}

		public bool AtEnd()
		{
			return next == null;
		}

		internal static Block FindBlock(Iterator<Line> state)
		{
			var currentIndention = state.Next().IndentionLevel;
			var r = new Block();
			while (!state.AtEnd() && !state.Next().IsEmptyLine() && state.Next().IndentionLevel >= currentIndention)
			{
				if (state.Next().IndentionLevel > currentIndention)
					r.Children.Add(FindBlock(state));
				else
				{
					r.Children.Add(state.Next());
					state.Advance();
				}
			}
			return r;
		}
	}
}
