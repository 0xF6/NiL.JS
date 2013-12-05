﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal sealed class Incriment : Operator
    {
        public Incriment(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            var val = (first ?? second).Invoke(context);
            if (val.ValueType == ObjectValueType.NoExist)
                throw new InvalidOperationException("varible is undefined");
            if ((val.assignCallback != null) && (!val.assignCallback()))
                return double.NaN;

            JSObject o = null;
            if ((second != null) && (val.ValueType != ObjectValueType.Undefined))
            {
                o = tempResult;
                o.Assign(val);
            }
            else
                o = val;
            if (val.ValueType == ObjectValueType.Int)
                val.iValue++;
            else if (val.ValueType == ObjectValueType.Double)
                val.dValue = val.dValue + 1.0;
            else if (val.ValueType == ObjectValueType.Bool)
            {
                val.ValueType = ObjectValueType.Int;
                val.iValue = val.bValue ? 2 : 1;
            }
            else if (val.ValueType == ObjectValueType.Undefined)
            {
                val.ValueType = ObjectValueType.Double;
                val.dValue = double.NaN;
            }
            else throw new NotImplementedException();
            return o;
        }

        public override bool Optimize(ref Statement _this, int depth, HashSet<string> vars)
        {
            if (depth <= 1 && second != null)
            {
                first = second;
                second = null;
            }
            return false;
        }
    }
}
