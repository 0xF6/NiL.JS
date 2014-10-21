﻿using System;
using NiL.JS.Core;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LogicalNot : Expression
    {
        public LogicalNot(CodeNode first)
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
                    _this = new ToBool((first as Expression).FirstOperand);
                    return true;
                }
                if (first.GetType() == typeof(Json))
                {
                    _this = new ImmidateValueStatement(false);
                    return true;
                }
                if (first.GetType() == typeof(ArrayStatement))
                {
                    _this = new ImmidateValueStatement(false);
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