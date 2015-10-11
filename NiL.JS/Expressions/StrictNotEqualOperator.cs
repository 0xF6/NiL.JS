﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class StrictNotEqualOperator : StrictEqualOperator
    {
        public StrictNotEqualOperator(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " !== " + second + ")";
        }
    }
}