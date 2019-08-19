using System;
using System.Collections.Generic;
using System.Text;

namespace ASS.Server.Extensions
{
    public static class ArrayExtension
    {
        public static T[] PreAppend<T>(this T[] array, params T[] toAppend)
        {
            if (array.Length == 0)
                return toAppend;
            if (toAppend.Length == 0)
                return array;
            var n = new T[toAppend.Length + array.Length];
            toAppend.CopyTo(n, 0);
            array.CopyTo(n, toAppend.Length);
            return n;
        }
    }
}
