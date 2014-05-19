using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct ExecutionLocation
    {
        public InstructionList Code;
        public int InstructionPointer;

        public ExecutionLocation(InstructionList Code, int InstructionPointer)
        {
            this.Code = Code;
            this.InstructionPointer = InstructionPointer;
        }

		public bool IsValid { get { return InstructionPointer >= 0 && InstructionPointer < Code.Count; } }
		public Instruction? Instruction { get { return Code[InstructionPointer] as Instruction?; } }

		public void Increment() 
		{ 
			InstructionPointer++;
		}

		public static ExecutionLocation Empty { get { return new ExecutionLocation(new InstructionList(), 0); } }

		public override string ToString()
		{
			return "CC(" + InstructionPointer + ")";
		}

		public static bool AreEqual(ExecutionLocation A, ExecutionLocation B)
		{
			return Object.ReferenceEquals(A.Code, B.Code) && A.InstructionPointer == B.InstructionPointer;
		}
    }
}
