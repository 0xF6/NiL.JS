﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalAnd : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LogicalAnd(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            var left = first.Evaluate(context);
            if (!(bool)left)
                return left;
            else
                return second.Evaluate(context);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " && " + second + ")";
        }
    }
}