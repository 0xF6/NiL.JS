﻿using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class LessOrEqual : More
    {
        public LessOrEqual(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var t = base.Invoke(context);
            t.iValue ^= 1;
            return t;
        }
    }
}