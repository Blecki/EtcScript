using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot.VirtualMachine
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
			while (IsValid && Code[InstructionPointer] is Annotation) 
				InstructionPointer++;  
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
		internal ScriptObject Frame = null;
		internal Stack<Object> Stack = new Stack<Object>();
		public ExecutionState ExecutionState { get; internal set; }
		public CodeContext CurrentInstruction;
		public Object R { get; internal set; }

		public Object Peek { get { return Stack.Peek(); } }

		public void Reset(ScriptObject GlobalScope, CodeContext ExecutionPoint)
		{
			Stack.Clear();
			Frame = new ScriptObject("@parent", GlobalScope);
			ExecutionState = ExecutionState.Running;
			CurrentInstruction = ExecutionPoint;
		}
		
		public ExecutionContext(ScriptObject GlobalScope, CodeContext ExecutionPoint)
		{
			Reset(GlobalScope, ExecutionPoint);
		}

	}
}
