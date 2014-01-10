using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
	internal class DeclarationIterator : Iterator<Declaration>
	{
		Iterator<Block> state;
		Declaration next;
		OperatorSettings operators;

		internal static DeclarationIterator Create(Iterator<Block> BlockIterator, OperatorSettings operators)
		{
			return new DeclarationIterator(BlockIterator, operators);
		}

		public DeclarationIterator(Iterator<Block> state, OperatorSettings operators)
		{
			this.state = state;
			this.operators = operators;
			Advance();
		}

		public Declaration Next()
		{
			return next;
		}

		public void Advance()
		{
			next = null;
			var wasError = false;

			do
			{
				wasError = false;
				if (!state.AtEnd())
				{
					try
					{
						next = Parser.BuildDeclaration(state.Next(), operators);
					}
					catch (CompileError e)
					{
						wasError = true;
						Console.WriteLine("Error: " + e.Message);
					}
					state.Advance();
				}
			} while (wasError);
		}

		public bool AtEnd()
		{
			return next == null;
		}
	}

}
