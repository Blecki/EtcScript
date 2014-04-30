using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class Compile
    {
		public static bool Debug = false;
		public static Action<String> _DebugWrite;

		public static void DebugWrite(String s)
		{
			if (_DebugWrite != null) _DebugWrite(s);
		}

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

		public static void EmitDebugDump(Declaration declaration)
		{
			DebugWrite(declaration.Type.ToString() + ":\n");
			DebugWrite(" TERMS: ");
			foreach (var term in declaration.Terms) DebugWrite(term.ToString() + " ");
			//Console.WriteLine();
			if (declaration.WhenClause != null)
			{
				//DebugWrite("\n WHEN AST:\n");
				//declaration.WhenClause.Body.Debug(0);
				DebugWrite("\n WHEN ENTRY: " + declaration.WhenClause.EntryPoint + "\n");
			}
			else
				DebugWrite("\n NO WHEN CLAUSE\n");
			//DebugWrite(" AST:\n");
			//declaration.Body.Body.Debug(0);
			DebugWrite(" ENTRY: " + declaration.Body.EntryPoint + "\n");
		}

		public static List<Declaration> Build(
			String script, 
			ParseContext context, 
			int StaticVariableOffset,
			Func<String, ErrorStrategy> OnError,
			bool DelayEmission = false)
		{
		    var r = Parse.Build(new TokenStream(new StringIterator(script), context), context, OnError);
			if (!DelayEmission) context.EmitDeclarations(StaticVariableOffset);
			return r;
		}

		private static int __contextID = 1;

		public static ParseContext GetDefaultParseContext(Environment Environment)
		{
			var context = Environment.Context;
			context.ID = ++__contextID;

			context.AddOperator(2, "|", VirtualMachine.InstructionSet.OR);
			context.AddOperator(0, "||", VirtualMachine.InstructionSet.LOR);
			context.AddOperator(0, "&&", VirtualMachine.InstructionSet.LAND);
			context.AddOperator(1, "==", VirtualMachine.InstructionSet.EQUAL);
			context.AddOperator(1, "!=", VirtualMachine.InstructionSet.NOT_EQUAL);
			context.AddOperator(1, "<", VirtualMachine.InstructionSet.LESS);
			context.AddOperator(1, ">", VirtualMachine.InstructionSet.GREATER);
			context.AddOperator(1, "<=", VirtualMachine.InstructionSet.LESS_EQUAL);
			context.AddOperator(1, ">=", VirtualMachine.InstructionSet.GREATER_EQUAL);
			context.AddOperator(2, "+", VirtualMachine.InstructionSet.ADD);
			context.AddOperator(2, "-", VirtualMachine.InstructionSet.SUBTRACT);
			context.AddOperator(2, "&", VirtualMachine.InstructionSet.AND);
			context.AddOperator(3, "*", VirtualMachine.InstructionSet.MULTIPLY);
			context.AddOperator(3, "/", VirtualMachine.InstructionSet.DIVIDE);
			context.AddOperator(3, "%", VirtualMachine.InstructionSet.MODULUS);

			foreach (var member in typeof(StandardLibrary).GetMethods())
				if (member.IsStatic)
					try
					{
						member.Invoke(null, new Object[] { Environment });
					}
					catch (Exception e) { }
			
			context.ActiveScope.Types.Add(Type.CreatePrimitive("NUMBER"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("STRING"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("COMPLEXSTRING"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("BOOLEAN"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("LIST"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("LAMBDA"));
			context.ActiveScope.Types.Add(Type.CreatePrimitive("CHAR"));
			context.ActiveScope.Types.Add(Type.Generic);
			context.ActiveScope.Types.Add(Type.Void);
			context.ActiveScope.Types.Add(Type.RuleResult);

			Environment.AddSystemVariable("stop", "RULE-RESULT", c => 0);
			Environment.AddSystemVariable("continue", "RULE-RESULT", c => 1);

			return context;
		}
    }
}
