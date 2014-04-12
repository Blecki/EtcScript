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

		public static Declaration CompileDeclaration(String data, Func<String, ErrorStrategy> OnError)
		{
			var operatorSettings = GetDefaultOperators();

			var tokenStream = new TokenStream(new StringIterator(data), operatorSettings);
			var ast = Parse.Build(tokenStream, operatorSettings, OnError);
			operatorSettings.EmitDeclarations();
			if (ast != null) foreach (var d in ast)
				{
					//d.EmitInstructions();
					//d.Body.Body.Debug(0);
					//EmitDeclarations(d);
					return d;
				}
			return null;
		}

		public static void EmitDebugDump(Declaration declaration)
		{
			DebugWrite(declaration.UsageSpecifier.ToString() + ":\n");
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
			Func<String, ErrorStrategy> OnError,
			bool DelayEmission = false)
		{
		    var r = Parse.Build(new TokenStream(new StringIterator(script), context), context, OnError);
			if (!DelayEmission) context.EmitDeclarations();
			return r;
		}

		private static int __contextID = 1;

		public static ParseContext GetDefaultOperators()
		{
			var operatorSettings = new ParseContext();
			operatorSettings.ID = ++__contextID;

			operatorSettings.AddOperator(1, "|", VirtualMachine.InstructionSet.OR);
			operatorSettings.AddOperator(0, "||", VirtualMachine.InstructionSet.LOR);
			operatorSettings.AddOperator(0, "&&", VirtualMachine.InstructionSet.LAND);
			operatorSettings.AddOperator(0, "==", VirtualMachine.InstructionSet.EQUAL);
			operatorSettings.AddOperator(0, "!=", VirtualMachine.InstructionSet.NOT_EQUAL);
			operatorSettings.AddOperator(0, "<", VirtualMachine.InstructionSet.LESS);
			operatorSettings.AddOperator(0, ">", VirtualMachine.InstructionSet.GREATER);
			operatorSettings.AddOperator(0, "<=", VirtualMachine.InstructionSet.LESS_EQUAL);
			operatorSettings.AddOperator(0, ">=", VirtualMachine.InstructionSet.GREATER_EQUAL);
			operatorSettings.AddOperator(1, "+", VirtualMachine.InstructionSet.ADD);
			operatorSettings.AddOperator(1, "-", VirtualMachine.InstructionSet.SUBTRACT);
			operatorSettings.AddOperator(1, "&", VirtualMachine.InstructionSet.AND);
			operatorSettings.AddOperator(2, "*", VirtualMachine.InstructionSet.MULTIPLY);
			operatorSettings.AddOperator(2, "/", VirtualMachine.InstructionSet.DIVIDE);
			operatorSettings.AddOperator(2, "%", VirtualMachine.InstructionSet.MODULUS);

			operatorSettings.AddControl(Control.Create(
				Declaration.Parse("foreach (x) in (list)"),
				ControlBlockType.RequiredBlock,
				(parameters, body) =>
				{
					if (parameters[0] is Ast.Identifier)
					{
						return new Ast.ForeachIn(parameters[0].Source,
							(parameters[0] as Ast.Identifier).Name.Value,
							parameters[1],
							body);
					}
					else
						throw new CompileError("Expected identifier", parameters[0].Source);
				}));

			operatorSettings.AddControl(Control.Create(
					Declaration.Parse("foreach (x) from (low) to (high)"),
					ControlBlockType.RequiredBlock,
					(parameters, body) =>
					{
						if (parameters[0] is Ast.Identifier)
						{
							return new Ast.ForeachFrom(parameters[0].Source,
								(parameters[0] as Ast.Identifier).Name.Value,
								parameters[1],
								parameters[2],
								body);
						}
						else
							throw new CompileError("Expected identifier", parameters[0].Source);
					}));

			operatorSettings.AddControl(Control.Create(
					Declaration.Parse("while (x)"),
					ControlBlockType.RequiredBlock,
					(parameters, body) =>
					{
						return new Ast.While(parameters[0].Source, parameters[0], body);
					}));

			operatorSettings.AddControl(Control.Create(
				Declaration.Parse("consider (x)"),
				ControlBlockType.NoBlock,
				(parameters, body) =>
				{
					return new Ast.ConsiderRule(parameters[0].Source, parameters[0]);
				}));

			operatorSettings.ActiveScope.Types.Add(Type.CreatePrimitive("NUMBER"));
			operatorSettings.ActiveScope.Types.Add(Type.CreatePrimitive("STRING"));

			return operatorSettings;
		}
    }
}
