using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum TypeOrigin
	{
		Script,
		System,
		Primitive
	}

	public class Type
	{
		public static bool AreTypesCompatible(Type Source, Type Destination)
		{
			if (Object.ReferenceEquals(Destination, Generic)) return true;
			return Object.ReferenceEquals(Source, Destination);
		}

		private static Type _void = new Type { Name = "VOID", Origin = TypeOrigin.Primitive };
		public static Type Void { get { return _void; } }

		private static Type _generic = new Type { Name = "GENERIC", Origin = TypeOrigin.System };
		public static Type Generic { get { return _generic; } }

		public static Type CreatePrimitive(String Name)
		{
			return new Type { Name = Name, Origin = TypeOrigin.Primitive };
		}

		public String Name;
		public TypeOrigin Origin;

		public List<Variable> Members = new List<Variable>();

		internal void AssignMemberOffsets()
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].Offset = i;
		}

		internal Variable FindMember(String Name)
		{
			return Members.FirstOrDefault(v => v.Name == Name);
		}

		internal int Size { get { return Members.Count; } }
	}
}
