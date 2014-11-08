﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class Xor : Expression
    {
        public Xor(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.iValue = Tools.JSObjectToInt32(first.Evaluate(context)) ^ Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Int;
            return tempContainer;
        }

        public override string ToString()
        {
            return "(" + first + " ^ " + second + ")";
        }
    }
}