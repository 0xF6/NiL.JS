﻿using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class MoreOrEqual : Less
    {
        public MoreOrEqual(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            var t = base.Invoke(context);
            t.iValue ^= 1;
            return t;
        }

        public override string ToString()
        {
            return "(" + first + " >= " + second + ")";
        }
    }
}