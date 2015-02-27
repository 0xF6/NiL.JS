﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ToUInt : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Number;
            }
        }

        public ToUInt(Expression first)
            : base(first, null, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var t = (uint)Tools.JSObjectToInt32(first.Evaluate(context));
            if (t <= int.MaxValue)
            {
                tempContainer.iValue = (int)t;
                tempContainer.valueType = JSObjectType.Int;
            }
            else
            {
                tempContainer.dValue = (double)t;
                tempContainer.valueType = JSObjectType.Double;
            }
            return tempContainer;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " | 0)";
        }
    }
}