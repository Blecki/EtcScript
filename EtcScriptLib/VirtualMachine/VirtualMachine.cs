using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
	public class RuntimeError : Exception
	{
		public RuntimeError(string msg, Instruction at)
			: base(msg)
		{
		}

		public RuntimeError(ExecutionContext context, string msg, Instruction at)
			: base(msg)
		{

		}
	}

    public class VirtualMachine
    {
		public static void ExecuteUntilFinished(ExecutionContext context)
		{
			while (context.ExecutionState == ExecutionState.Running)
				ExecuteSingleInstruction(context);
		}

		public static bool DetailedTracing = false;
		public static Action<String> WriteTraceLine;

		private static void Trace(ExecutionContext Context)
		{
			System.Diagnostics.Debug.Assert(WriteTraceLine != null);

			var str = "";

			var Instruction = Context.CurrentInstruction.Instruction.Value;
			
			str += Instruction.Opcode.ToString();
			
			int ip = Context.CurrentInstruction.InstructionPointer;
			if (Instruction.FirstOperand != Operand.NONE)
			{
				str += " " + TraceOperand(Context, Instruction.FirstOperand, out ip, ip);
				if (Instruction.SecondOperand != Operand.NONE)
				{
					str += " " + TraceOperand(Context, Instruction.SecondOperand, out ip, ip);
					if (Instruction.ThirdOperand != Operand.NONE)
					{
						str += " " + TraceOperand(Context, Instruction.ThirdOperand, out ip, ip);
					}
				}
			}

			str += "  R: " + (Context.R == null ? "null" : Context.R.ToString()) + "  F: " + Context.F.ToString();

			str += "  STACK TOP:";

			for (int i = Context.Stack.Count - 1; i >= 0 && i > Context.Stack.Count - 10; --i)
				str += " [" + (Context.Stack[i] == null ? "null" : Context.Stack[i].ToString()) + "]";

			WriteTraceLine(str);
		}

		private static String TraceOperand(ExecutionContext Context, Operand Operand, out int NewIP, int IP)
		{
			NewIP = IP;
			if (Operand == EtcScriptLib.VirtualMachine.Operand.NEXT)
			{
				NewIP = IP + 1;
				return Context.CurrentInstruction.Code[NewIP] == null ? "null" : Context.CurrentInstruction.Code[NewIP].ToString();
			}
			else if (Operand == EtcScriptLib.VirtualMachine.Operand.STRING)
			{
				NewIP = IP + 1;
				return "STRING[" + Context.CurrentInstruction.Code[NewIP].ToString() + "]";
			}
			else
				return Operand.ToString();
		}
		
        public static void ExecuteSingleInstruction(ExecutionContext context)
        {
			if (context.CurrentInstruction.Code == null) throw new InvalidProgramException("Null code");

            //When an error represents bad output from the compiler or a built in function,
            //      an exception is thrown in C#. 
            //When an error represents faulty input code, an exception is thrown in MISP.

            if (context.ExecutionState != ExecutionState.Running) return;

			if (!context.CurrentInstruction.IsValid)
			{
				context.ExecutionState = ExecutionState.Finished;
				return;
			}

			//Console.Write(context.CurrentInstruction.InstructionPointer + " ");
			if (DetailedTracing) Trace(context);

			var instructionStart = context.CurrentInstruction;
			var nextInstruction = context.CurrentInstruction.Instruction;
			context.CurrentInstruction.Increment();
            if (!nextInstruction.HasValue) 
                throw new InvalidOperationException("Encountered non-code in instruction stream");
            var ins = nextInstruction.Value;

			//Console.WriteLine("I: " + ins.ToString() + " S: " +
			//	String.Join("  ", context.Stack.Select(o => o.ToString())));

            try
            {
                switch (ins.Opcode)
                {
                    #region MOVE
                    case InstructionSet.MOVE:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            SetOperand(ins.SecondOperand, v, context);
                        }
                        break;
                    #endregion
						
					#region RSOs

					case InstructionSet.ALLOC_RSO:
						SetOperand(ins.SecondOperand, 
							new RuntimeScriptObject((GetOperand(ins.FirstOperand, context) as int?).Value),
							context);
						break;
					case InstructionSet.STORE_RSO_M:
						{
							var v = GetOperand(ins.FirstOperand, context);
							var rso = GetOperand(ins.SecondOperand, context) as RuntimeScriptObject;
							var m = GetOperand(ins.ThirdOperand, context) as int?;
							rso.Data[m.Value] = v;
						}
						break;
					case InstructionSet.LOAD_RSO_M:
						{
							var rso = GetOperand(ins.FirstOperand, context) as RuntimeScriptObject;
							var m = GetOperand(ins.SecondOperand, context) as int?;
							SetOperand(ins.ThirdOperand, rso.Data[m.Value], context);
						}
						break;

					#endregion

					#region Flow Control

					case InstructionSet.RETURN:
                        context.CurrentInstruction = (GetOperand(ins.FirstOperand, context) as CodeContext?).Value;
                        break;
                    case InstructionSet.CLEANUP:
                        {
                            var count = (GetOperand(ins.FirstOperand, context) as int?).Value;
							context.Stack.RemoveRange(context.Stack.Count - count, count);
                        }
                        break;

					case InstructionSet.JUMP:
						{
							var destination = GetOperand(ins.FirstOperand, context) as int?;
							if (destination.HasValue) context.CurrentInstruction.InstructionPointer = destination.Value;
						}
						break;
					
                    #endregion

                    #region Lambdas

					case InstructionSet.COMPAT_INVOKE:
						{
							var argumentCount = Convert.ToInt32(GetOperand(ins.FirstOperand, context));
							var arguments = new List<Object>();
							for (int i = 0; i < argumentCount; ++i)
								arguments.Insert(0, GetOperand(Operand.POP, context));

							if (argumentCount == 0) throw new InvalidOperationException();
							var function = arguments[0] as InvokeableFunction;
							if (function == null) throw new InvalidOperationException();

							try
							{
								var result = function.Invoke(context, arguments);
								if (result.InvokationSucceeded != true)
									throw new InvalidOperationException();
							}
							catch (Exception e)
							{
								Throw(new RuntimeError(e.Message, ins), context);
								break;
							}
						}
						break;

					case InstructionSet.STACK_INVOKE:
						{
							var invokable = GetOperand(ins.FirstOperand, context) as InvokeableFunction;

							if (invokable == null)
							{
								Throw(new RuntimeError("Attempted to invoke what isn't a function", ins), context);
								break;
							}

							System.Diagnostics.Debug.Assert(invokable.IsStackInvokable);

							SetOperand(Operand.PUSH, context.CurrentInstruction, context);
							invokable.StackInvoke(context);
						}
						break;

					case InstructionSet.CALL:
						{
							var target = GetOperand(ins.FirstOperand, context) as int?;
							SetOperand(Operand.PUSH, context.CurrentInstruction, context);
							context.CurrentInstruction.InstructionPointer = target.Value;
						}
						break;


					case InstructionSet.LAMBDA:
						{
							var rso = GetOperand(ins.FirstOperand, context) as RuntimeScriptObject;
							var lambda = LambdaFunction.CreateTrueLambda(context.CurrentInstruction, rso);
							SetOperand(ins.SecondOperand, lambda, context);
						}
						break;
                    
					case InstructionSet.MARK_STACK:
						SetOperand(ins.FirstOperand, context.Stack.Count, context);
						break;

					case InstructionSet.RESTORE_STACK:
						{
							var goal = GetOperand(ins.FirstOperand, context) as int?;
							if (goal < context.Stack.Count)
								context.Stack.RemoveRange(goal.Value, context.Stack.Count - goal.Value);
						}
						break;

					//case InstructionSet.SET_FRAME:
					//    {
					//        var frame = GetOperand(ins.FirstOperand, context) as ScriptObject;
					//        context.Frame = frame;
					//    }
					//    break;

                    #endregion

                    #region Lists
                    case InstructionSet.EMPTY_LIST:
                        SetOperand(ins.FirstOperand, new List<Object>(), context);
                        break;
                    case InstructionSet.APPEND:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            var l = GetOperand(ins.SecondOperand, context) as List<Object>;
                            l.Add(v);
                            SetOperand(ins.ThirdOperand, l, context);
                        }
                        break;
                    
					#endregion

					#region Variables

					case InstructionSet.LOAD_PARAMETER:
						{
							var offset = GetOperand(ins.FirstOperand, context) as int?;
							SetOperand(ins.SecondOperand, context.Stack[context.F + offset.Value], context);
						}
						break;

					case InstructionSet.STORE_PARAMETER:
						{
							var value = GetOperand(ins.FirstOperand, context);
							var offset = GetOperand(ins.SecondOperand, context) as int?;
							context.Stack[context.F + offset.Value] = value;
						}
						break;

					case InstructionSet.LOAD_STATIC:
						{
							var offset = GetOperand(ins.FirstOperand, context) as int?;
							if (context.StaticVariables.ContainsKey(offset.Value))
								SetOperand(ins.SecondOperand, context.StaticVariables[offset.Value], context);
							else
								SetOperand(ins.SecondOperand, null, context);
						}
						break;

					case InstructionSet.STORE_STATIC:
						{
							var value = GetOperand(ins.FirstOperand, context);
							var offset = GetOperand(ins.SecondOperand, context) as int?;
							context.StaticVariables.Upsert(offset.Value, value);
						}
						break;

                    #endregion

                    #region Loop control
                    case InstructionSet.DECREMENT:
                        {
							var v = Convert.ToInt32(GetOperand(ins.FirstOperand, context));
                            SetOperand(ins.SecondOperand, v - 1, context);
                        }
                        break;
                    case InstructionSet.INCREMENT:
                        {
								var v = Convert.ToInt32(GetOperand(ins.FirstOperand, context));
								SetOperand(ins.SecondOperand, v + 1, context);
                        }
                        break;
                    case InstructionSet.LESS:
                        {
                            dynamic v0 = GetOperand(ins.FirstOperand, context);
                            dynamic v1 = GetOperand(ins.SecondOperand, context);
                            var result = v0 < v1;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;
					case InstructionSet.GREATER:
						{
							dynamic v0 = GetOperand(ins.FirstOperand, context);
							dynamic v1 = GetOperand(ins.SecondOperand, context);
							var result = v0 > v1;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;
					case InstructionSet.LESS_EQUAL:
						{
							dynamic v0 = GetOperand(ins.FirstOperand, context);
							dynamic v1 = GetOperand(ins.SecondOperand, context);
							var result = v0 <= v1;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;
                    case InstructionSet.GREATER_EQUAL:
                        {
                            dynamic v0 = GetOperand(ins.FirstOperand, context);
                            dynamic v1 = GetOperand(ins.SecondOperand, context);
                            var result = v0 >= v1;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;
                    case InstructionSet.IF_TRUE:
                        {
                            var b = GetOperand(ins.FirstOperand, context) as bool?;
                            if (!b.HasValue || !b.Value) Skip(context);
                        }
                        break;
                    case InstructionSet.IF_FALSE:
                        {
                            var b = GetOperand(ins.FirstOperand, context) as bool?;
                            if (b.HasValue && b.Value) Skip(context);
                        }
                        break;
                    case InstructionSet.SKIP:
                        {
                            var distance = (GetOperand(ins.FirstOperand, context) as int?).Value;
                            while (distance > 0) { Skip(context); --distance; }
                        }
                        break;

                    case InstructionSet.EQUAL:
                        {
							try
							{
								dynamic a = GetOperand(ins.FirstOperand, context);
								dynamic b = GetOperand(ins.SecondOperand, context);
								var result = (a == b);
								SetOperand(ins.ThirdOperand, result, context);
							}
							catch (Exception e)
							{
								SetOperand(ins.ThirdOperand, false, context);
							}
                        }
                        break;

					case InstructionSet.NOT_EQUAL:
						{
							dynamic a = GetOperand(ins.FirstOperand, context);
							dynamic b = GetOperand(ins.SecondOperand, context);
							var result = (a != b);
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

                    #endregion

                    #region Error Handling
                    case InstructionSet.CATCH:
                        {
							var returnPoint = instructionStart;
                            var handler = GetOperand(ins.FirstOperand, context) as InstructionList;
                            var code = GetOperand(ins.SecondOperand, context) as InstructionList;
                            SetOperand(Operand.PUSH, returnPoint, context); //Push the return point
							var catchContext = new ErrorHandler(new CodeContext(handler, 0));
                            SetOperand(Operand.PUSH, catchContext, context);
                            context.CurrentInstruction = new CodeContext(code, 0);
                        }
                        break;
                    case InstructionSet.THROW:
                        {
                            var errorObject = GetOperand(ins.FirstOperand, context);
                            Throw(errorObject, context);
                        }
                        break;
                    #endregion

                    #region Maths
                    case InstructionSet.ADD:
                        {
                            dynamic first = GetOperand(ins.FirstOperand, context);
                            dynamic second = GetOperand(ins.SecondOperand, context);
                            var result = first + second;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;
                    case InstructionSet.SUBTRACT:
                        {
                            dynamic first = GetOperand(ins.FirstOperand, context);
                            dynamic second = GetOperand(ins.SecondOperand, context);
                            var result = first - second;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;
                    case InstructionSet.MULTIPLY:
                        {
                            dynamic first = GetOperand(ins.FirstOperand, context);
                            dynamic second = GetOperand(ins.SecondOperand, context);
                            var result = first * second;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;
                    case InstructionSet.DIVIDE:
                        {
                            dynamic first = GetOperand(ins.FirstOperand, context);
                            dynamic second = GetOperand(ins.SecondOperand, context);
                            var result = first / second;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;

					case InstructionSet.MODULUS:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = first % second;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

					case InstructionSet.AND:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = first & second;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

					case InstructionSet.OR:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = first | second;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

					case InstructionSet.LOR:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = first || second;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

					case InstructionSet.LAND:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = first && second;
							SetOperand(ins.ThirdOperand, result, context);
						}
						break;

                    #endregion

                    case InstructionSet.DEBUG:
                        Console.WriteLine(GetOperand(ins.FirstOperand, context));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                Throw(new RuntimeError(e.Message + e.StackTrace + "\nBEFORE: " +
					context.CurrentInstruction.InstructionPointer + 
					"\nINSTRUCTION: [" + ins.ToString() + "]\nSTACK DUMP:\n" +
					String.Join("\n", context.Stack.Select((o)=> { return o == null ? "NULL" : o.ToString(); }))
				, ins), context);
            }
        }

        public static void Throw(Object errorObject, ExecutionContext context)
        {
			context.ErrorObject = errorObject;

            while (true)
            {
                if (context.Stack.Count == 0)
                {
                    context.ExecutionState = ExecutionState.Error;
                    break;
                }

				var topOfStack = context.Stack[context.Stack.Count - 1];
				context.Stack.RemoveAt(context.Stack.Count - 1);
                if (topOfStack is ErrorHandler)
                {
                    var handler = (topOfStack as ErrorHandler?).Value;
                    context.CurrentInstruction = handler.HandlerCode;
					//context.Frame = handler.ParentScope;
					//context.Frame["error"] = errorObject;
					break;
                }
            }
        }

        public static void SetOperand(Operand operand, Object value, ExecutionContext context)
        {
            switch (operand)
            {
                case Operand.NEXT: throw new InvalidOperationException("Can't set to next");
                case Operand.NONE: break; //Silently ignore.
				case Operand.PEEK: context.Stack[context.Stack.Count - 1] = value; break;
                case Operand.POP: throw new InvalidOperationException("Can't set to pop");
                case Operand.PUSH: context.Stack.Add(value); break;
				case Operand.R: context.R = value; break;
				case Operand.STRING: throw new InvalidOperationException("Can't set to the string table");
				case Operand.F: 
					if (value is int) 
						context.F = (value as int?).Value; 
					else throw new InvalidOperationException("F can only be an integer"); 
					break;
            }
        }

        public static Object GetOperand(Operand operand, ExecutionContext context)
        {
            switch (operand)
            {
                case Operand.NEXT: return context.CurrentInstruction.Code[context.CurrentInstruction.InstructionPointer++];
                case Operand.NONE: throw new InvalidOperationException("Can't fetch from nothing");
				case Operand.PEEK: return context.Stack[context.Stack.Count - 1];
				case Operand.POP: 
					{ 
						var r = context.Stack[context.Stack.Count - 1]; 
						context.Stack.RemoveAt(context.Stack.Count - 1); 
						return r; 
					}
                case Operand.PUSH: throw new InvalidOperationException("Can't fetch from push");
				case Operand.R: return context.R;
				case Operand.STRING:
					{
						var tableIndex = context.CurrentInstruction.Code[context.CurrentInstruction.InstructionPointer++] as int?;
						if (!tableIndex.HasValue) throw new InvalidOperationException("Index into string table was not integer");
						return context.CurrentInstruction.Code.StringTable[tableIndex.Value];
					}
				case Operand.F: return context.F;
                default: throw new InvalidProgramException();
            }
        }

        public static void Skip(ExecutionContext context)
        {
            if (!context.CurrentInstruction.IsValid)
            {
                context.ExecutionState = ExecutionState.Finished;
                return;
            }

			var nextInstruction = context.CurrentInstruction.Instruction;
			context.CurrentInstruction.Increment();
            if (!nextInstruction.HasValue) 
                throw new RuntimeError("Encountered non-code in instruction stream", new Instruction());
            var ins = nextInstruction.Value;

			if (ins.FirstOperand == Operand.NEXT || ins.FirstOperand == Operand.STRING) context.CurrentInstruction.Increment();
			if (ins.SecondOperand == Operand.NEXT || ins.SecondOperand == Operand.STRING) context.CurrentInstruction.Increment();
			if (ins.ThirdOperand == Operand.NEXT || ins.ThirdOperand == Operand.STRING) context.CurrentInstruction.Increment();
        }
    }
}
