using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public struct CodeContext
    {
        public InstructionList Code;
        public int InstructionPointer;

        public CodeContext(InstructionList Code, int InstructionPointer)
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

		public static CodeContext Empty { get { return new CodeContext(new InstructionList(), 0); } }

		public override string ToString()
		{
			return "CC(" + InstructionPointer + ")";
		}
    }

    public enum ExecutionState
    {
        Running,
        Finished,
        Blocked,
        Error
    }

	public class ExecutionContext
	{
		internal List<Object> Stack = new List<Object>();
		public Object R;
		public int F;
		public ExecutionState ExecutionState { get; set; }
		public CodeContext CurrentInstruction;
		public Object Tag;

		public Object ErrorObject;

		public Object Peek { get { return Stack.Count > 0 ? Stack[Stack.Count - 1] : null; } }

		public void Reset(CodeContext ExecutionPoint)
		{
			Stack.Clear();
			Stack.Add(new CodeContext(new InstructionList(), 0)); //Fake return point
			R = null;
			ExecutionState = ExecutionState.Running;
			CurrentInstruction = ExecutionPoint;
		}
		
		public ExecutionContext(CodeContext ExecutionPoint)
		{
			Reset(ExecutionPoint);
		}

	}
}
