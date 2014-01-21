using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum ErrorStrategy
	{
		Continue,
		Abort
	}

	internal class DeclarationIterator : Iterator<Declaration>
	{
		Iterator<Block> state;
		Declaration next;
		OperatorSettings operators;
		public Func<String, ErrorStrategy> OnError;

		internal static DeclarationIterator Create(
			Iterator<Block> BlockIterator,
			OperatorSettings operators,
			Func<String, ErrorStrategy> OnError)
		{
			var r = new DeclarationIterator
			{
				state = BlockIterator,
				operators = operators,
				OnError = OnError
			};
			r.Advance();
			return r;
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
						if (OnError != null)
						{
							var strategy = OnError(e.Message);
							if (strategy == ErrorStrategy.Abort)
							{
								next = null;
								return;
							}
						}
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
