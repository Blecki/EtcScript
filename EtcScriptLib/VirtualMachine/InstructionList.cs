using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public class InstructionList
    {
		public List<Object> Data = new List<Object>();
		public StringTable StringTable = new StringTable();

		public int Count { get { return Data.Count; } }
		public Object this[int key]
		{
			get { return Data[key]; }
			set { Data[key] = value; }
		}

		public void AddInstruction(InstructionSet Opcode, Operand First, Operand Second, Operand Third)
		{
			Data.Add(new Instruction(Opcode, First, Second, Third));
		}

        public void AddInstructions(params Object[] instructions)
        {
            int literalsExpected = 0;
            foreach (var instruction in instructions)
            {
	            if (literalsExpected > 0)
                {
                    --literalsExpected;
					Data.Add(instruction);
                    continue;
                }

				if (instruction is Instruction)
				{
					var parsedInstruction = (instruction as Instruction?).Value;
					Data.Add(parsedInstruction);
					literalsExpected += CountLiteralOperands(parsedInstruction);
				}
                else if (instruction is String)
                {
                    var parsedInstruction = Instruction.Parse(instruction.ToString());
					Data.Add(parsedInstruction);
					literalsExpected += CountLiteralOperands(parsedInstruction);
				}
                else
                    throw new InvalidOperationException("Was not expecting a literal");
            }
        }

		private int CountLiteralOperands(Instruction Instruction)
		{
			int literals = 0;
			if (ExpectLiteral(Instruction.FirstOperand)) ++literals;
			if (ExpectLiteral(Instruction.SecondOperand)) ++literals;
			if (ExpectLiteral(Instruction.ThirdOperand)) ++literals;
			return literals;
		}

		private bool ExpectLiteral(Operand Operand)
		{
			if (Operand == EtcScriptLib.VirtualMachine.Operand.NEXT) return true;
			if (Operand == EtcScriptLib.VirtualMachine.Operand.STRING) return true;
			return false;
		}

        public InstructionList(params Object[] instructions)
        {
            AddInstructions(instructions);
        }

		public int AddString(String str)
		{
			return StringTable.Add(str);
		}

		
	}
}
