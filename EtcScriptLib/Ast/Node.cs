using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.Ast
{
	public enum OperationDestination
	{
		R,
		Stack,
		Discard,
		Top,
	}

	public class Node
	{
		public static VirtualMachine.Operand WriteOperand(OperationDestination Destination)
		{
			switch (Destination)
			{
				case OperationDestination.R:
					return VirtualMachine.Operand.R;
				case OperationDestination.Stack:
					return VirtualMachine.Operand.PUSH;
				case OperationDestination.Top:
					return VirtualMachine.Operand.PEEK;
				case OperationDestination.Discard:
				default:
					throw new InvalidOperationException();
			}
		}
		public static VirtualMachine.Operand ReadOperand(OperationDestination Destination)
		{
			switch (Destination)
			{
				case OperationDestination.R:
					return VirtualMachine.Operand.R;
				case OperationDestination.Stack:
					return VirtualMachine.Operand.POP;
				case OperationDestination.Top:
					return VirtualMachine.Operand.PEEK;
				case OperationDestination.Discard:
				default:
					throw new InvalidOperationException();
			}
		}

		public Token Source;
		public Type ResultType;
		public bool IsAssignmentTarget = false;

		public Node(Token Source)
		{
			this.Source = Source;
		}

		public virtual void Emit(VirtualMachine.InstructionList Instructions, OperationDestination Destination)
		{
			throw new NotImplementedException();
		}

		public virtual void Debug(int depth)
		{
			throw new NotImplementedException();
		}

		public virtual Node Transform(ParseScope Scope)
		{
			throw new NotImplementedException();
		}

		public class DummyKeyword : Node
		{
			public String Name;
			public DummyKeyword(String Name) : base(new Token()) { this.Name = Name; }
		}

		public class DummyTerm : Node
		{
			public DummyTerm(Type Type) : base(new Token()) { this.ResultType = Type; }
		}

		public class DummyTermOfAnyType : Node
		{
			public DummyTermOfAnyType() : base(new Token()) { }
		}

		public static DummyKeyword Keyword(String Name) { return new DummyKeyword(Name); }
		public static DummyTerm Term(Type Type) { return new DummyTerm(Type); }
		public static DummyTermOfAnyType TermOfAnyType() { return new DummyTermOfAnyType(); }
		public static List<Node> DummyArguments(params Node[] Arguments) { return new List<Node>(Arguments); }
		public static bool ExactDummyMatch(List<DeclarationTerm> Terms, List<Node> Arguments)
		{
			if (Terms.Count != Arguments.Count) return false;
			for (int i = 0; i < Terms.Count; ++i)
			{
				if (Terms[i].Type == DeclarationTermType.Keyword || Terms[i].Type == DeclarationTermType.Operator)
				{
					var dummyKeyword = Arguments[i] as DummyKeyword;
					if (dummyKeyword == null || (dummyKeyword.Name != Terms[i].Name)) return false;
				}
				else if (Terms[i].Type == DeclarationTermType.Term)
				{
					var dummyTerm = Arguments[i] as DummyTerm;
					if (dummyTerm != null)
					{
						if (!System.Object.ReferenceEquals(dummyTerm.ResultType, Terms[i].DeclaredType))
							return false;
					}
					else if (Arguments[i] is DummyTermOfAnyType)
						return true;
					else
						return false;
				}
				else
					throw new InvalidOperationException();
			}
			return true;
		}

	}
}
