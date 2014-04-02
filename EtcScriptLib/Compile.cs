using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib
{
    public class Compile
    {
        private static void EmitDeclarations(Declaration node)
        {
            Console.WriteLine(node.UsageSpecifier.ToString() + ": ");
			VirtualMachine.Debug.DumpOpcode(node.Body.Instructions, Console.Out, 1);
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
			declaration.Body.Body.Debug(0);
			EmitDeclarations(declaration);
		}

		public static List<Declaration> Build(
			String script, 
			ParseContext context, 
			Func<String, ErrorStrategy> OnError)
		{
		    return Parse.Build(new TokenStream(new StringIterator(script), context), context, OnError);
		}

		public static ParseContext GetDefaultOperators()
		{
			var operatorSettings = new ParseContext();

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

			return operatorSettings;
		}
    }
}
