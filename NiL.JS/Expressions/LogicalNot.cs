﻿using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalNot : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LogicalNot(Expression first)
            : base(first, null, false)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return !(bool)first.Evaluate(context);
        }

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict)
        {
            var res = base.Build(ref _this, depth, vars, strict);
            if (!res)
            {
                if (first.GetType() == typeof(LogicalNot))
                {
                    _this = new ToBool((first).FirstOperand);
                    return true;
                }
                if (first.GetType() == typeof(Json))
                {
                    _this = new Constant(false);
                    return true;
                }
                if (first.GetType() == typeof(ArrayStatement))
                {
                    _this = new Constant(false);
                    return true;
                }
                if (first.GetType() == typeof(Equal))
                {
                    _this = new NotEqual((first).FirstOperand, (first).SecondOperand);
                    return true;
                }
                if (first.GetType() == typeof(More))
                {
                    _this = new LessOrEqual((first).FirstOperand, (first).SecondOperand);
                    return true;
                }
                if (first.GetType() == typeof(Less))
                {
                    _this = new MoreOrEqual((first).FirstOperand, (first).SecondOperand);
                    return true;
                }
                if (first.GetType() == typeof(MoreOrEqual))
                {
                    _this = new Less((first).FirstOperand, (first).SecondOperand);
                    return true;
                }
                if (first.GetType() == typeof(LessOrEqual))
                {
                    _this = new More((first).FirstOperand, (first).SecondOperand);
                    return true;
                }
            }
            return res;
        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}