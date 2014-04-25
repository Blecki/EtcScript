using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public partial class StandardLibrary
	{
		public static void ListFunctions(Environment Environment)
		{
			Environment.AddSystemMacro(
				"length of (l:list) : number", 
				(c, a) => { return (int)(a[0] as List<Object>).Count; });
			
			Environment.AddSystemMacro(
				"(l:list) at (n:number) : generic", 
				(c, a) => { return (a[0] as List<Object>)[Convert.ToInt32(a[1])]; });

			Environment.AddSystemMacro(
				"set at (n:number) on (l:list) to (v:generic)",
				(c, a) => { (a[1] as List<Object>)[Convert.ToInt32(a[0])] = a[2]; return null; });

			Environment.AddSystemMacro(
				"get at (n:number) from (l:list) : generic",
				(c, a) => { return (a[1] as List<Object>)[Convert.ToInt32(a[0])]; });
			
			Environment.AddSystemMacro(
				"append (v:generic) to (l:list) : list", 
				(c, a) =>
			{
				var r = a[1] == null ? new List<Object>() : new List<Object>(a[1] as List<Object>);
				r.Add(a[0]);
				return r;
			});

			Environment.AddSystemMacro(
				"remove (v:generic) from (l:list) : list",
				(c, a) =>
				{
					return new List<Object>((a[1] as List<Object>).Where(o => !Object.ReferenceEquals(o, a[0])));
				});

			Environment.AddSystemMacro(
				"insert (v:generic) into (l:list) at (i:number) : list",
				(c, a) =>
				{
					var r = new List<Object>(a[1] as List<Object>);
					r.Insert(Convert.ToInt32(a[2]), a[0]);
					return r;
				});

		}
	}
}
