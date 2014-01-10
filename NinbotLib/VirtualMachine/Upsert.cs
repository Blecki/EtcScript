using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninbot.VirtualMachine
{
    public static class HelperExtensions
    {
        public static void Upsert<A, B>(this Dictionary<A, B> Dict, A _a, B _b)
        {
            if (Dict.ContainsKey(_a)) Dict[_a] = _b;
            else Dict.Add(_a, _b);
        }

        /// <summary>
        /// Add a range of values to a list. Eliminates need to create temporary array to pass to AddRange.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="list"></param>
        /// <param name="values"></param>
        public static void AddMany<V>(this List<V> list, params V[] values)
        {
            list.AddRange(values);
        }
    }
}
