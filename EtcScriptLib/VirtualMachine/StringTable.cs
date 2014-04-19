using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public struct Part
	{
		public int Start;
		public int Length;

		public override string ToString()
		{
			return "" + Start + " : " + Length;
		}
	}

	public class StringTable
    {
		public List<String> RawStringTable = new List<string>();
		public Part[] PartTable;
		public String StringData;

		public String this[int i]
		{
			get
			{
				var part = PartTable[i];
				return StringData.Substring(part.Start, part.Length);
			}
		}

		public int Add(String str)
		{
			//Prevent duplicate part entries for identical strings.
			var r = RawStringTable.IndexOf(str);
			if (r >= 0) return r;
			r = RawStringTable.Count;
			RawStringTable.Add(str);
			return r;
		}

		private class Entry
		{
			internal String RawString;
			internal int OriginalIndex;
		}

		private class EntryLengthComparer : IComparer<Entry>
		{
			public int Compare(Entry x, Entry y)
			{
				return y.RawString.Length - x.RawString.Length;
			}
		}

		public void Compress()
		{
			PartTable = new Part[RawStringTable.Count];
			StringData = "";

			var entries = new List<Entry>(RawStringTable.Select((s, i) => new Entry { RawString = s, OriginalIndex = i }));
			entries.Sort(new EntryLengthComparer());

			foreach (var entry in entries)
			{
				var substringStart = StringData.IndexOf(entry.RawString);
				if (substringStart < 0)
				{
					substringStart = StringData.Length;
					StringData += entry.RawString;
				}

				PartTable[entry.OriginalIndex] = new Part { Start = substringStart, Length = entry.RawString.Length };
			}
		}		
	}
}
