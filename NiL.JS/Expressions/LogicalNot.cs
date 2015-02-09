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

        internal override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> vars, bool strict, CompilerMessageCallback message, FunctionStatistic statistic, OptimizationOptions opts)
        {
            var res = base.Build(ref _this, depth, vars, strict, message, statistic, opts);
            if (!res)
            {
                if (first.GetType() == typeof(LogicalNot))
                {
                    _this = new ToBool((first).FirstOperand) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(Json))
                {
                    _this = new Constant(false) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(ArrayExpression))
                {
                    _this = new Constant(false) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(Equal))
                {
                    _this = new NotEqual((first).FirstOperand, (first).SecondOperand) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(More))
                {
                    _this = new LessOrEqual((first).FirstOperand, (first).SecondOperand) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(Less))
                {
                    _this = new MoreOrEqual((first).FirstOperand, (first).SecondOperand) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(MoreOrEqual))
                {
                    _this = new Less((first).FirstOperand, (first).SecondOperand) { Position = Position, Length = Length };
                    return true;
                }
                if (first.GetType() == typeof(LessOrEqual))
                {
                    _this = new More((first).FirstOperand, (first).SecondOperand) { Position = Position, Length = Length };
                    return true;
                }
            }
            return res;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}