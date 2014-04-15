﻿using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    public sealed class Mod : Operator
    {
        public Mod(Statement first, Statement second)
            : base(first, second)
        {

        }

        internal override JSObject Invoke(Context context)
        {
            double left = Tools.JSObjectToDouble(first.Invoke(context));
            tempResult.dValue = left % Tools.JSObjectToDouble(second.Invoke(context));
            tempResult.ValueType = JSObjectType.Double;
            return tempResult;
        }

        public override string ToString()
        {
            return "(" + first + " % " + second + ")";
        }
    }
}