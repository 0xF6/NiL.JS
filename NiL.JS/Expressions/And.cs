﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class And : Expression
    {
        public And(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = Tools.JSObjectToInt32(first.Evaluate(context));
            tempContainer.iValue = left & Tools.JSObjectToInt32(second.Evaluate(context));
            tempContainer.valueType = JSObjectType.Int;
            return tempContainer;
        }

        public override string ToString()
        {
            return "(" + first + " & " + second + ")";
        }
    }
}