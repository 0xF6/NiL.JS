﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Statements.Operators
{
    [Serializable]
    public sealed class LogicalOr : Operator
    {
        public LogicalOr(Statement first, Statement second)
            : base(first, second, true)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var left = first.Invoke(context);

            if ((bool)left)
                return left;
            else
                return second.Invoke(context);
        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}