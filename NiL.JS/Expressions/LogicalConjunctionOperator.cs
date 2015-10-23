﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class LogicalConjunctionOperator : Expression
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public LogicalConjunctionOperator(Expression first, Expression second)
            : base(first, second, false)
        {

        }

        public override JSValue Evaluate(Context context)
        {
            var left = first.Evaluate(context);
            if (!(bool)left)
                return left;
            else
                return second.Evaluate(context);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, System.Collections.Generic.Dictionary<string, VariableDescriptor> variables, BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (message != null && depth <= 1)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, 0), "Do not use logical operator as a conditional statement");
            return base.Build(ref _this, depth, variables, state | BuildState.Conditional, message, statistic, opts);
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