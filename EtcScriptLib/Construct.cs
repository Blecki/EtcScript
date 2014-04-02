using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	internal class Construct
	{
		public virtual Token FindFirstToken()
		{
			throw new NotImplementedException();
		}
	}

	internal class Line : Construct
	{
		public int IndentionLevel;
		public List<Token> Tokens;

		public Line(int IndentionLevel)
		{
			this.IndentionLevel = IndentionLevel;
			this.Tokens = new List<Token>();
		}

		public bool IsEmptyLine()
		{
			return Tokens.Count == 0;
		}

		internal TokenIterator GetIterator()
		{
			return new TokenIterator { source = this, tokens = Tokens };
		}

		public override Token FindFirstToken()
		{
			if (IsEmptyLine()) throw new InvalidProgramException();
			return Tokens[0];
		}
	}

	internal class Block : Construct
	{
		public List<Construct> Children = new List<Construct>();
		public Iterator<Construct> GetIterator() { return Children.GetIterator(); }
		public override Token FindFirstToken()
		{
			if (Children.Count == 0) throw new InvalidProgramException();
			return Children[0].FindFirstToken();
		}
	}

	internal class TokenIterator : Iterator<Token>
	{
		public Line source;
		public List<Token> tokens;
		public int place = 0;

		public Token Next()
		{
			return tokens[place];
		}

		public void Advance()
		{
			++place;
		}

		public bool AtEnd() { return place >= tokens.Count; }
	}

	internal class ListIterator<T> : Iterator<T>
	{
		internal List<T> _storage;
		internal int _place = 0;

		public T Next()
		{
			return _storage[_place];
		}

		public void Advance()
		{
			++_place;
		}

		public bool AtEnd()
		{
			return _place >= _storage.Count;
		}
	}

	internal static class ListIteratorExtension
	{
		internal static ListIterator<T> GetIterator<T>(this List<T> l)
		{
			return new ListIterator<T> { _storage = l };
		}
	}
}
