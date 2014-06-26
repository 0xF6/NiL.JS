﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class UnsignedShiftRight : Expression
    {
        public UnsignedShiftRight(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            lock (this)
            {
                var left = Tools.JSObjectToInt32(first.Invoke(context));
                tempContainer.dValue = (double)((uint)left >> Tools.JSObjectToInt32(second.Invoke(context)));
                tempContainer.valueType = JSObjectType.Double;
                return tempContainer;
            }
        }

        public override string ToString()
        {
            return "(" + first + " >>> " + second + ")";
        }
    }
}