using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class Debug
    {
        internal static void DumpOpcode(List<Object> opcode, System.IO.TextWriter to, int indent)
        {
			var iterator = opcode.GetIterator();

			while (!iterator.AtEnd())
			{
				var instruction = iterator.Next();
				to.Write(new String(' ', indent * 4));
				if (instruction == null) to.Write("NULL\n");
				else if (instruction is List<String>)
                {
                    to.Write("[ ");
					foreach (var entry in instruction as List<String>) to.Write(entry + " ");
                    to.Write("]\n");
                }
				else if (instruction is List<Object>)
                {
                    to.Write("--- Embedded instruction stream\n");
					DumpOpcode(instruction as List<Object>, to, indent + 1);
                    to.Write(new String(' ', indent * 4));
                    to.Write("--- End embedded stream\n");
                }
				else if (instruction is String)
					to.Write("\"" + instruction.ToString() + "\"\n");
				else if (instruction is Instruction)
				{
					var ins = (instruction as Instruction?).Value;
					to.Write(iterator._place.ToString() + ": " + ins.Opcode.ToString() + "  ");
					to.Write(GetOperandString(ins.FirstOperand, iterator) + "  ");
					to.Write(GetOperandString(ins.SecondOperand, iterator) + "  ");
					to.Write(GetOperandString(ins.ThirdOperand, iterator) + "\n");
				}
				else to.Write(instruction.ToString() + "\n");

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
			else if (operand == Operand.NONE)
				return "";
			return operand.ToString();
		}
    }
}
