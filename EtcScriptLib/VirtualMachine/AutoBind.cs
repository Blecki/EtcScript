using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptLib.VirtualMachine
{
    public class AutoBind
    {
        private static List<Object> MakeList(params Object[] items)
        {
            return new List<Object>(items);
        }

        public static List<Object> ListArgument(Object obj)
        {
            if (obj is List<Object>) return obj as List<Object>;
            return MakeList(obj);
        }

        public static float NumericArgument(Object obj)
        {
            return Convert.ToSingle(obj);
        }

        public static int IntArgument(Object obj)
        {
			if (obj == null) return 0;
            return Convert.ToInt32(obj);
        }

        public static char CharArgument(Object obj)
        {
            return Convert.ToChar(obj);
        }

        public static uint UIntArgument(Object obj)
        {
            return Convert.ToUInt32(obj);
        }

        public static bool BooleanArgument(Object obj)
        {
            return Convert.ToBoolean(obj);
        }

        public static string StringArgument(Object obj)
        {
            return obj.ToString();
        }

        public static InstructionList LazyArgument(Object obj)
        {
            return obj as InstructionList;
        }

        public static NativeFunction GenerateMethodBinding(System.Reflection.MethodInfo method)
        {
            return new NativeFunction((context, arguments) =>
                {
                    var parameters = method.GetParameters();
                    int start = method.IsStatic ? 0 : 1;
                    var transformedArguments = new Object[arguments.Count - start];
                    for (var i = 0; i < transformedArguments.Length; ++i)
                        transformedArguments[i] = Convert.ChangeType(arguments[i + start], parameters[i].ParameterType);
                    return method.Invoke(method.IsStatic ? null : arguments[0], transformedArguments);
                });
        }
    }
}
