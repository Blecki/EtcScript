using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{

	public class StringIterator : Iterator<int>
	{
		private String data;
		private int place = 0;

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
	}

}
