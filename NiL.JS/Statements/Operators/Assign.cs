﻿using NiL.JS.Core;
using System;

namespace NiL.JS.Statements.Operators
{
    internal class Assign : Operator
    {
        private static JSObject setterArgs = new JSObject(true) { ValueType = JSObjectType.Object, oValue = "[object Arguments]" };
        private static JSObject setterArg = new JSObject();

        static Assign()
        {
            setterArgs.fields["length"] = new JSObject() { iValue = 1, ValueType = JSObjectType.Int, assignCallback = JSObject.ProtectAssignCallback };
            setterArgs.fields["0"] = setterArg;
        }

        public Assign(Statement first, Statement second)
            : base(first, second)
        {
        }

        public override JSObject Invoke(Context context)
        {
            setterArg.Assign(second.Invoke(context));
            var field = first.InvokeForAssing(context);
            if (field.ValueType == JSObjectType.Property)
            {
                var setter = (field.oValue as NiL.JS.Core.BaseTypes.Function[])[0];
                if (setter != null)
                    setter.Invoke(context, setterArgs);
            }
            else
                field.Assign(setterArg);
            return setterArg;
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            var res = base.Optimize(ref _this, depth, vars);
            var t = first;
            while (t is Operators.None)
                t = (t as Operators.None).Second;
            if (t is Operators.Call)
                throw new InvalidOperationException("Invalid left-hand side in assignment.");
            return res;
        }
    }
}