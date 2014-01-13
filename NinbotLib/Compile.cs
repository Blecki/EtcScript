using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot
{
    public class Compile
    {
        private static void EmitDeclarations(Declaration node)
        {
            Console.WriteLine(node.Type.ToString() + ": " + node.Name);
			VirtualMachine.Debug.DumpOpcode(node.Instructions, Console.Out, 1);
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
            var ast = Parser.Build(tokenStream, operatorSettings, OnError);
			if (ast != null) foreach (var d in ast)
				{
					EmitDeclarations(d);
					return d;
				}
			return null;
        }

		public static List<Declaration> Build(String script, Func<String, ErrorStrategy> OnError)
		{
			var ops = GetDefaultOperators();
			return Parser.Build(new TokenStream(new StringIterator(script), ops), ops, OnError);
		}

		public static OperatorSettings GetDefaultOperators()
		{
			var operatorSettings = new OperatorSettings();

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

			return operatorSettings;
		}
    }
}
