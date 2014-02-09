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
	}

    public class VirtualMachine
    {
		public static void ExecuteUntilFinished(ExecutionContext context)
		{
			while (context.ExecutionState == ExecutionState.Running)
				ExecuteSingleInstruction(context);
		}

        public static void ExecuteSingleInstruction(ExecutionContext context)
        {
            //When an error represents bad output from the compiler or a built in function,
            //      an exception is thrown in C#. 
            //When an error represents faulty input code, an exception is thrown in MISP.

            if (context.ExecutionState != ExecutionState.Running) return;

			while (context.CurrentInstruction.IsValid 
				&& context.CurrentInstruction.Code[context.CurrentInstruction.InstructionPointer] is Annotation)
				++context.CurrentInstruction.InstructionPointer;

			if (!context.CurrentInstruction.IsValid)
			{
				context.ExecutionState = ExecutionState.Finished;
				return;
			}

			var instructionStart = context.CurrentInstruction;
			var nextInstruction = context.CurrentInstruction.Instruction;
			context.CurrentInstruction.Increment();
            if (!nextInstruction.HasValue) 
                throw new InvalidOperationException("Encountered non-code in instruction stream");
            var ins = nextInstruction.Value;

            try
            {
                switch (ins.Opcode)
                {
                    case InstructionSet.YIELD:
                        return;
					case InstructionSet.SWAP_TOP:
						{
							var A = GetOperand(Operand.POP, context);
							var B = GetOperand(Operand.POP, context);
							SetOperand(Operand.PUSH, A, context);
							SetOperand(Operand.PUSH, B, context);
						}
						break;

                    #region MOVE
                    case InstructionSet.MOVE:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            SetOperand(ins.SecondOperand, v, context);
                        }
                        break;
                    #endregion

                    #region Lookup
                    case InstructionSet.LOOKUP:
                        {
                            var name = GetOperand(ins.FirstOperand, context).ToString();
                            Object value = null;

                            if (!context.Frame.QueryProperty(name, out value))
                            {
                                Throw(new RuntimeError("Could not resolve name '" + name + "'.", nextInstruction.Value), context);
                                break;
                            }

                            SetOperand(ins.SecondOperand, value, context);
                        }
                        break;
					#endregion

					#region LOOKUP_MEMBER
					case InstructionSet.LOOKUP_MEMBER:
                        {
                            var memberName = GetOperand(ins.FirstOperand, context).ToString();
                            var obj = GetOperand(ins.SecondOperand, context);
                            Object value = null;

                            if (obj == null)
                            {
                                Throw(new RuntimeError("Could not inspect members of NULL.", nextInstruction.Value), context);
                                break;
                            }

                            if (obj is ScriptObject) //Special handling of script objects.
                            {
                                var scriptObject = obj as ScriptObject;
                                if (!scriptObject.QueryProperty(memberName, out value))
                                {
                                    Throw(new RuntimeError("Could not find member " + memberName + " on generic object.", nextInstruction.Value),
                                        context);
                                    break;
                                }
                            }
                            else
                            {
                                var lookupResult = LookupMemberWithReflection(obj, memberName);
                                if (lookupResult.FoundMember == false)
                                {
                                    Throw(new RuntimeError("Could not find member " + memberName + " on type " +
                                        obj.GetType().Name + ".", nextInstruction.Value), context);
                                    break;
                                }
                                else
                                    value = lookupResult.Member;
                            }

                            SetOperand(ins.ThirdOperand, value, context);
                        }
                        break;

                    case InstructionSet.SET_MEMBER:
                        {
                            var value = GetOperand(ins.FirstOperand, context);
                            var memberName = GetOperand(ins.SecondOperand, context).ToString();
                            var obj = GetOperand(ins.ThirdOperand, context);


                            if (obj == null)
                            {
                                Throw(new RuntimeError("Could not set members of NULL.", nextInstruction.Value), context);
                                break;
                            }

                            if (obj is ScriptObject) //Special handling of script objects.
                            {
                                var scriptObject = obj as ScriptObject;
                                scriptObject.SetProperty(memberName, value);
                            }
                            else
                            {
                                var lookupResult = SetMemberWithReflection(obj, memberName, value);
                                if (lookupResult.FoundMember == false)
                                {
                                    Throw(new RuntimeError("Could not find settable member " + memberName + " on type " +
                                        obj.GetType().Name + ".", nextInstruction.Value), context);
                                    break;
                                }
                            }
                        }
                        break;

                    case InstructionSet.RECORD:
                        SetOperand(ins.FirstOperand, new ScriptObject(), context);
                        break;
                    #endregion

                    #region Flow Control

                    case InstructionSet.MARK:
                        {
                            var storedContext = context.CurrentInstruction;
                            SetOperand(ins.FirstOperand, storedContext, context);
                        }
                        break;
                    case InstructionSet.BREAK:
                        {
                            var breakContext = GetOperand(ins.FirstOperand, context);
                            context.CurrentInstruction = (breakContext as CodeContext?).Value;
                            Skip(context); //Skip the instruction stored by BRANCH
                        }
                        break;
                    
                    case InstructionSet.BRANCH:
                        SetOperand(ins.FirstOperand, instructionStart, context);
                        context.CurrentInstruction = new CodeContext(GetOperand(ins.SecondOperand, context) as InstructionList, 0);
                        break;
                    case InstructionSet.CONTINUE:
                        context.CurrentInstruction = (GetOperand(ins.FirstOperand, context) as CodeContext?).Value;
                        break;
                    case InstructionSet.CLEANUP:
                        {
                            var count = (GetOperand(ins.FirstOperand, context) as int?).Value;
                            while (count > 0)
                            {
                                context.Stack.Pop();
                                --count;
                            }
                        }
                        break;

					case InstructionSet.JUMP:
						{
							var destination = GetOperand(ins.FirstOperand, context) as int?;
							if (destination.HasValue) context.CurrentInstruction.InstructionPointer = destination.Value;
						}
						break;
					case InstructionSet.JUMP_RELATIVE:
						{
								var destination = GetOperand(ins.FirstOperand, context) as int?;
							if (destination.HasValue) context.CurrentInstruction.InstructionPointer += destination.Value;
						}
						break;
					case InstructionSet.JUMP_MARK:
						{
							var destination = GetOperand(ins.FirstOperand, context) as int?;
							SetOperand(ins.SecondOperand, context.CurrentInstruction, context);
							if (destination.HasValue) context.CurrentInstruction.InstructionPointer = destination.Value;
						}
						break;

                    #endregion

                    #region Lambdas

                    case InstructionSet.INVOKE:
                        {
                            var arguments = GetOperand(ins.FirstOperand, context) as List<Object>;

                            if (arguments == null)
                            {

                                Throw(new RuntimeError("Argument list is null? ", ins), context);
                                break;
                            }

                            if (arguments.Count == 0 || arguments[0] == null)
                            {
                                Throw(new RuntimeError("Attempted to invoke... nothing.", ins), context);
                                break;
                            }

                            var function = arguments[0];
                            if (function is InvokeableFunction)
                            {
								//SetOperand(Operand.PUSH, instructionStart, context); //Push return point.

                                try
                                {
                                    var InvokationResult = (function as InvokeableFunction).Invoke(context, arguments);
                                    if (InvokationResult.InvokationSucceeded == false)
                                    {
                                        Throw(new RuntimeError(InvokationResult.ErrorMessage + " thrown by " + function.ToString(), ins), context);
                                        break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Throw(new RuntimeError(e.Message + " attempting to invoke " + function.ToString(), ins),
                                        context);
                                }
                            }
                            else
                            {
                                if (arguments.Count > 1)
                                    Throw(new RuntimeError(
                                        "If the first argument in a node isn't an invokeable function, there can't " +
                                        "be any further arguments.", nextInstruction.Value), context);
                                else
                                    SetOperand(Operand.PUSH, arguments[0], context);
                            }
                        }
                        break;

                    case InstructionSet.LAMBDA:
                        {
							var arguments = GetOperand(ins.FirstOperand, context) as List<String>;
							var code = GetOperand(ins.SecondOperand, context) as InstructionList;
							var lambda = LambdaFunction.CreateLambda("", code, context.Frame, arguments);
							SetOperand(ins.ThirdOperand, lambda, context);
                        }
                        break;
                    
					case InstructionSet.MARK_STACK:
						SetOperand(ins.FirstOperand, context.Stack.Count, context);
						break;

					case InstructionSet.RESTORE_STACK:
						{
							var goal = GetOperand(ins.FirstOperand, context) as int?;
							if (goal.HasValue)
								while (context.Stack.Count > goal.Value) context.Stack.Pop();
						}
						break;

					case InstructionSet.SET_FRAME:
						{
							var frame = GetOperand(ins.FirstOperand, context) as ScriptObject;
							context.Frame = frame;
						}
						break;

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
                    case InstructionSet.APPEND_RANGE:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            var l = GetOperand(ins.SecondOperand, context) as List<Object>;
                            l.AddRange(v as List<Object>);
                            SetOperand(ins.ThirdOperand, l, context);
                        }
                        break;

                    case InstructionSet.PREPEND:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            var l = GetOperand(ins.SecondOperand, context) as List<Object>;
                            l.Insert(0, v);
                            SetOperand(ins.ThirdOperand, l, context);
                        }
                        break;
                    case InstructionSet.PREPEND_RANGE:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            var l = GetOperand(ins.SecondOperand, context) as List<Object>;
                            l.InsertRange(0, v as List<Object>);
                            SetOperand(ins.ThirdOperand, l, context);
                        }
                        break;

                    case InstructionSet.LENGTH:
                        {
							var v = GetOperand(ins.FirstOperand, context);
							if (v is List<Object>)
								SetOperand(ins.SecondOperand, (v as List<Object>).Count, context);
							else
								SetOperand(ins.SecondOperand, 1, context);
                        }
                        break;

                    case InstructionSet.INDEX:
                        {
                            var i = GetOperand(ins.FirstOperand, context) as int?;
                            var l = GetOperand(ins.SecondOperand, context) as List<Object>;
                            //Console.WriteLine(i.ToString() + " " + l.Count.ToString());
                            SetOperand(ins.ThirdOperand, l[i.Value], context);
                        }
                        break;
                    #endregion

                    #region Variables
                    //case InstructionSet.PUSH_VARIABLE:
                    //    {
                    //        var v = GetOperand(ins.FirstOperand, context);
                    //        var name = GetOperand(ins.SecondOperand, context).ToString();
                    //        context.Scope.PushVariable(name, v);
                    //    }
                    //    break;
                    case InstructionSet.SET_VARIABLE:
                        {
                            var v = GetOperand(ins.FirstOperand, context);
                            var name = GetOperand(ins.SecondOperand, context).ToString();
                            context.Frame[name] = v;
                        }
                        break;
                    //case InstructionSet.POP_VARIABLE:
                    //    {
                    //        var name = GetOperand(ins.FirstOperand, context).ToString();
                    //        var v = context.Scope.GetVariable(name);
                    //        context.Scope.PopVariable(name);
                    //        SetOperand(ins.SecondOperand, v, context);
                    //    }
                    //    break;
                    #endregion

                    #region Loop control
                    case InstructionSet.DECREMENT:
                        {
                            var v = GetOperand(ins.FirstOperand, context) as int?;
                            SetOperand(ins.SecondOperand, v.Value - 1, context);
                        }
                        break;
                    case InstructionSet.INCREMENT:
                        {
                            var v = GetOperand(ins.FirstOperand, context) as int?;
                            SetOperand(ins.SecondOperand, v.Value + 1, context);
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
                            dynamic a = GetOperand(ins.FirstOperand, context);
                            dynamic b = GetOperand(ins.SecondOperand, context);
                            var result = (a == b);
                            SetOperand(ins.ThirdOperand, result, context);
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
                            var catchContext = new ErrorHandler(new CodeContext(handler, 0), context.Frame);
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
                            var result = second - first;
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
                            var result = second / first;
                            SetOperand(ins.ThirdOperand, result, context);
                        }
                        break;

					case InstructionSet.MODULUS:
						{
							dynamic first = GetOperand(ins.FirstOperand, context);
							dynamic second = GetOperand(ins.SecondOperand, context);
							var result = second % first;
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
                Throw(new RuntimeError(e.Message + " " + ins.ToString(), ins), context);
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

                var topOfStack = context.Stack.Pop();
                if (topOfStack is ErrorHandler)
                {
                    var handler = (topOfStack as ErrorHandler?).Value;
                    context.CurrentInstruction = handler.HandlerCode;
					context.Frame = handler.ParentScope;
					context.Frame["error"] = errorObject;
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
                case Operand.PEEK: context.Stack.Pop(); context.Stack.Push(value); break;
                case Operand.POP: throw new InvalidOperationException("Can't set to pop");
                case Operand.PUSH: context.Stack.Push(value); break;
				case Operand.R: context.R = value; break;
            }
        }

        public static Object GetOperand(Operand operand, ExecutionContext context)
        {
            switch (operand)
            {
                case Operand.NEXT: return context.CurrentInstruction.Code[context.CurrentInstruction.InstructionPointer++];
                case Operand.NONE: throw new InvalidOperationException("Can't fetch from nothing");
                case Operand.PEEK: return context.Stack.Peek();
                case Operand.POP: return context.Stack.Pop();
                case Operand.PUSH: throw new InvalidOperationException("Can't fetch from push");
				case Operand.R: return context.R;
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

			if (ins.FirstOperand == Operand.NEXT) context.CurrentInstruction.Increment();
            if (ins.SecondOperand == Operand.NEXT) context.CurrentInstruction.Increment();
            if (ins.ThirdOperand == Operand.NEXT) context.CurrentInstruction.Increment();
        }

        public struct MemberLookupResult
        {
            public bool FoundMember;
            public Object Member;

            public static MemberLookupResult Success(Object Member) { return new MemberLookupResult { Member = Member, FoundMember = true }; }
            public static MemberLookupResult Failure { get { return new MemberLookupResult { FoundMember = false }; }}
        }

        public static MemberLookupResult LookupMemberWithReflection(Object obj, String memberName)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            var type = obj.GetType();

            var field = type.GetField(memberName);
            if (field != null)
                return MemberLookupResult.Success(field.GetValue(obj));

            var property = type.GetProperty(memberName);
            if (property != null)
                return MemberLookupResult.Success(property.GetValue(obj, null));

            var methods = type.FindMembers(
                System.Reflection.MemberTypes.Method,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                new System.Reflection.MemberFilter((minfo, _obj) => { return minfo.Name == memberName; }),
                memberName);
            
			if (methods.Length != 0)
                return MemberLookupResult.Success(new OverloadedReflectionFunction(obj, memberName));

            return MemberLookupResult.Failure;
        }

        public static MemberLookupResult SetMemberWithReflection(Object obj, String memberName, Object value)
        {
            System.Diagnostics.Debug.Assert(obj != null);

            var type = obj.GetType();

            var field = type.GetField(memberName);
            if (field != null)
            {
                field.SetValue(obj, value);
                return MemberLookupResult.Success(value);
            }

            var property = type.GetProperty(memberName);
            if (property != null)
            {
                property.SetValue(obj, value, null);
                return MemberLookupResult.Success(value);
            }

            return MemberLookupResult.Failure;
        }
    }
}
