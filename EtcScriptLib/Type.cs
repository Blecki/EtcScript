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

	public struct TypeCompatibilityResult
	{
		public bool Compatible;
		public bool ConversionRequired;
		public Declaration ConversionMacro;

		public static TypeCompatibilityResult NoConversionRequired = new TypeCompatibilityResult
		{
			Compatible = true,
			ConversionRequired = false
		};

		public static TypeCompatibilityResult Incompatible = new TypeCompatibilityResult
		{
			Compatible = false
		};
	}

	public class Type
	{
		#region Compatibility and Conversion
		public static TypeCompatibilityResult AreTypesCompatible(Type Source, Type Destination, ParseScope Scope)
		{
			if (Object.ReferenceEquals(Destination, Generic)) return TypeCompatibilityResult.NoConversionRequired;
			if (Object.ReferenceEquals(Source, Destination)) return TypeCompatibilityResult.NoConversionRequired;

			var super = Source.Super;
			while (super != null)
			{
				if (Object.ReferenceEquals(super, Destination)) return TypeCompatibilityResult.NoConversionRequired;
				super = super.Super;
			}

			var conversionArguments = new List<Ast.Node>();
			conversionArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "convert" }));
			conversionArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "0" }) { ResultType = Source });
			conversionArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = "to" }));
			conversionArguments.Add(new Ast.Identifier(new Token { Type = TokenType.Identifier, Value = Destination.Name }));

			var possibleConversions = Scope.FindAllPossibleMacroMatches(conversionArguments);
			Declaration matchingConversion = null;
			foreach (var possibleConversion in possibleConversions)
				if (Object.ReferenceEquals(Source, possibleConversion.Terms[1].DeclaredType))
				{
					matchingConversion = possibleConversion;
					break;
				}

			if (matchingConversion != null)
			{
				if (!Object.ReferenceEquals(matchingConversion.ReturnType, Destination))
					throw new CompileError("Conversion function does not return the correct type.",
						matchingConversion.Body.Body.Source);

				return new TypeCompatibilityResult
				{
					Compatible = true,
					ConversionRequired = true,
					ConversionMacro = matchingConversion
				};
			}

			return TypeCompatibilityResult.Incompatible;
		}

		public static Ast.Node CreateConversionInvokation(
			ParseScope Scope,
			Declaration ConversionMacro,
			Ast.Node Value)
		{
			return Ast.StaticInvokation.CreateCorrectInvokationNode(Value.Source, Scope, ConversionMacro,
						new List<Ast.Node>(new Ast.Node[] { Value }));
		}
		#endregion

		#region Builtin types
		private static Type _void = new Type(null) { Name = "VOID", Origin = TypeOrigin.Primitive };
		public static Type Void { get { return _void; } }

		private static Type _generic = new Type(null) { Name = "GENERIC", Origin = TypeOrigin.System };
		public static Type Generic { get { return _generic; } }

		private static Type _rule_result = new Type(null) { Name = "RULE-RESULT", Origin = TypeOrigin.Primitive };
		public static Type RuleResult { get { return _rule_result; } }

		public static Type CreatePrimitive(String Name)
		{
			return new Type(null) { Name = Name, Origin = TypeOrigin.Primitive };
		}
		#endregion

		public String Name;
		public TypeOrigin Origin;
		public Type Super;
		public String SuperTypename;
		public int ID;
		public String Documentation;

		public String Description { get { return Name + " : " + (Super == null ? "GENERIC" : Super.Name) + " - " + Origin; } }

		public List<Variable> Members = new List<Variable>();

		public Type(String Super)
		{
			this.SuperTypename = Super;
		}

		internal void AssignMemberOffsets()
		{
			for (int i = 0; i < Members.Count; ++i)
				Members[i].Offset = (Super == null ? i : (i + Super.Size));
		}

		internal Variable FindMember(String Name)
		{
			var r = Members.FirstOrDefault(v => v.Name == Name);
			if (r == null && Super != null) return Super.FindMember(Name);
			return r;
		}

		internal int Size
		{
			get
			{
				var r = Members.Count;
				if (Super != null) r += Super.Size;
				else r += 1; //Top level types need 1 slot for their type info.
				return r;
			}
		}

		internal static void ThrowConversionError(Type SourceType, Type DestinationType, Token Source)
		{
			if (SourceType == null) throw new InvalidOperationException("Conversion Error : SourceType is null");
			if (DestinationType == null) throw new InvalidOperationException("Conversion Error : DestinationType is null");
			throw new CompileError("Incompatible types: Unable to convert from " + SourceType.Name + " to " +
				DestinationType.Name + ".", Source);
		}

		public void ResolveTypes(ParseScope ActiveScope)
		{
			if (String.IsNullOrEmpty(SuperTypename))
				Super = null;
			else
			{
				Super = ActiveScope.FindType(SuperTypename);
				if (Super == null) throw new CompileError("Could not find type '" + SuperTypename + "'.");
			}

			if (Super == null)
				Members.Insert(0, new Variable
				{
					Name = "__type",
					DeclaredTypeName = "NUMBER",
					StorageMethod = VariableStorageMethod.Member
				});

			var search = Super;
			while (search != null)
			{
				if (Object.ReferenceEquals(search, this)) throw new CompileError("Inheritance loop detected.");
				search = search.Super;
			}

			if (Origin == TypeOrigin.Script)
				foreach (var member in Members)
				{
					if (String.IsNullOrEmpty(member.DeclaredTypeName))
						member.DeclaredType = Type.Generic;
					else
					{
						member.DeclaredType = ActiveScope.FindType(member.DeclaredTypeName);
						if (member.DeclaredType == null) throw new CompileError("Could not find type '" +
							member.DeclaredTypeName + "'.");
					}
				}
		}
	}
}
