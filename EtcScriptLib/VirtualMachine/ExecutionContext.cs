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
		public ScriptObject Frame { get; internal set; }
		internal Stack<Object> Stack = new Stack<Object>();
		public Object R;
		public ExecutionState ExecutionState { get; set; }
		public CodeContext CurrentInstruction;
		public Object Tag;

		public Object ErrorObject;

		public Object Peek { get { return Stack.Count > 0 ? Stack.Peek() : null; } }

		public void Reset(ScriptObject GlobalScope, CodeContext ExecutionPoint)
		{
			Stack.Clear();
			Stack.Push(new CodeContext(new InstructionList(), 0)); //Fake return point
			R = null;
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
