using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
	public enum VariableStorageMethod
	{
		Local,
		Member,
		Static,
		System,
		LambdaCapture
	}

	public class Variable
	{
		public String Name;
		public int Offset;
		public Type DeclaredType;
		public String DeclaredTypeName;
		public VariableStorageMethod StorageMethod = VariableStorageMethod.Local;

		public void ResolveType(ParseScope ActiveScope)
		{
			if (String.IsNullOrEmpty(DeclaredTypeName))
				DeclaredType = Type.Generic;
			else
			{
				DeclaredType = ActiveScope.FindType(DeclaredTypeName);
				if (DeclaredType == null) throw new CompileError("Could not find type '" + DeclaredTypeName + "'.");
			}
		}
	}
}
