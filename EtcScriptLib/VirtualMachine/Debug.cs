using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class Debug
    {
        internal static void DumpOpcode(List<Object> opcode, Action<String> Write, int indent)
        {
			var iterator = opcode.GetIterator();

			while (!iterator.AtEnd())
			{
				var instruction = iterator.Next();
				Write(new String(' ', indent * 4));
				if (instruction == null) Write("NULL\n");
				else if (instruction is List<String>)
                {
                    Write("[ ");
					foreach (var entry in instruction as List<String>) Write(entry + " ");
                    Write("]\n");
                }
				else if (instruction is List<Object>)
                {
                    Write("--- Embedded instruction stream\n");
					DumpOpcode(instruction as List<Object>, Write, indent + 1);
                    Write(new String(' ', indent * 4));
                    Write("--- End embedded stream\n");
                }
				else if (instruction is String)
					Write("\"" + instruction.ToString() + "\"\n");
				else if (instruction is Instruction)
				{
					var ins = (instruction as Instruction?).Value;
					Write(iterator._place.ToString() + ": " + ins.Opcode.ToString() + "  ");
					Write(GetOperandString(ins.FirstOperand, iterator) + "  ");
					Write(GetOperandString(ins.SecondOperand, iterator) + "  ");
					Write(GetOperandString(ins.ThirdOperand, iterator) + "  ");
					if (!String.IsNullOrEmpty(ins.Annotation)) Write("#" + ins.Annotation);
					Write("\n");
				}
				else Write(instruction.ToString() + "\n");

				iterator.Advance();
            }
        }

		private static string GetOperandString(Operand operand, ListIterator<object> iterator)
		{
			if (operand == Operand.NEXT)
			{
				iterator.Advance();
				return iterator.Next().ToString();
			}
			else if (operand == Operand.STRING)
			{
				iterator.Advance();
				return "STRING[" + iterator.Next().ToString() + "]";
			}
			else if (operand == Operand.NONE)
				return "";
			return operand.ToString();
		}
    }
}
