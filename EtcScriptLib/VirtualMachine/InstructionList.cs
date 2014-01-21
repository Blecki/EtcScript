using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public class Annotation
	{
		public String text;
		public static Annotation Create(String text)
		{
			return new Annotation { text = text };
		}

		public override string ToString()
		{
			return "[" + text + "]";
		}
	}

    public class InPlace : InstructionList
    {
        public InPlace(params Object[] instructions)
            : base(instructions)
        { }

        public InPlace(InstructionList compiledInstructions)
        {
            this.AddRange(compiledInstructions);
        }
    }

    public class InstructionList : List<Object>
    {
        private int SourceLine = 0;

        public void AddInstruction(params Object[] instructions)
        {
            int literalsExpected = 0;
            foreach (var instruction in instructions)
            {
				if (instruction is Annotation)
				{
					this.Add(instruction);
					continue;
				}

                if (literalsExpected > 0)
                {
                    --literalsExpected;
                    this.Add(instruction);
                    continue;
                }

				if (instruction is Instruction)
				{
					var parsedInstruction = (instruction as Instruction?).Value;
					this.Add(parsedInstruction);
					if (parsedInstruction.FirstOperand == Operand.NEXT) ++literalsExpected;
					if (parsedInstruction.SecondOperand == Operand.NEXT) ++literalsExpected;
					if (parsedInstruction.ThirdOperand == Operand.NEXT) ++literalsExpected;
				}
                else if (instruction is String)
                {
                    var parsedInstruction = Instruction.Parse(instruction.ToString());
                    this.Add(parsedInstruction);
                    if (parsedInstruction.FirstOperand == Operand.NEXT) ++literalsExpected;
                    if (parsedInstruction.SecondOperand == Operand.NEXT) ++literalsExpected;
                    if (parsedInstruction.ThirdOperand == Operand.NEXT) ++literalsExpected;
                }
                else if (instruction is InPlace)
                {
                    this.AddRange(instruction as InstructionList);
                }
                else
                    throw new InvalidOperationException("Was not expecting a literal");
            }
        }

        public InstructionList(params Object[] instructions)
        {
            AddInstruction(instructions);
        }
    }
}
