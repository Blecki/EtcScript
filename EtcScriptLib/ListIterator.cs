using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
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
