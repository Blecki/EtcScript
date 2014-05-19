using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public enum ExecutionState
    {
        Running,
        Finished,
        Blocked,
        Error
    }

	public class ExecutionContext
	{
		internal Dictionary<int,Object> StaticVariables = null;
		internal List<Type> Types = null;
		internal List<Object> Stack = new List<Object>();
		public Object R;
		public int F;
		public ExecutionState ExecutionState { get; set; }
		public ExecutionLocation CurrentInstruction;
		public Object Tag;

		public Object ErrorObject;
		public Object Peek { get { return Stack.Count > 0 ? Stack[Stack.Count - 1] : null; } }

		internal ExecutionContext(Dictionary<int,Object> StaticVariables, List<Type> Types, ExecutionLocation ExecutionPoint)
		{
			this.StaticVariables = StaticVariables;
			this.Types = Types;
			Stack.Add(new ExecutionLocation(new InstructionList(), 0)); //Fake return point
			R = null;
			ExecutionState = ExecutionState.Running;
			CurrentInstruction = ExecutionPoint;
		}

	}
}
