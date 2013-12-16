﻿using NiL.JS.Core.BaseTypes;
using NiL.JS.Core;
using System;

namespace NiL.JS.Statements
{
    internal sealed class ImmidateValueStatement : Statement, IOptimizable, IContextStatement
    {
        public readonly JSObject Value;

        public ImmidateValueStatement(JSObject value)
        {
            Value = value;
            value.temporary = false;
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            return Value;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            return Value;
        }

        public JSObject Invoke()
        {
            return Value;
        }

        public JSObject Invoke(JSObject _this, IContextStatement[] args)
        {
            return Value;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            var vss = Value.Value as Statement[];
            if (vss != null)
            {
                for (int i = 0; i < vss.Length; i++)
                    Parser.Optimize(ref vss[i], depth + 1, varibles);
            }
            return false;
        }
    }
}