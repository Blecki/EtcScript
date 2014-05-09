using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{

	public class StringIterator : Iterator<int>
	{
		internal String data;
		internal int place = 0;

		public int Next()
		{
			return data[place];
		}

		public void Advance()
		{
			++place;
		}

		public bool AtEnd()
		{
			return place >= data.Length;
		}

		public StringIterator(String data)
		{
			this.data = data;
		}

		public StringIterator(String data, int place)
		{
			this.data = data;
			this.place = place;
		}
	}

}
