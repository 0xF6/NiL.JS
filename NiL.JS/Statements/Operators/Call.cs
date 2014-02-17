﻿using NiL.JS.Core;
using System;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements.Operators
{
    internal class Call : Operator
    {
        private Statement[] args;

        public Call(Statement first, Statement second)
            : base(first, second)
        {

        }

        public override JSObject Invoke(Context context)
        {
            JSObject newThisBind = null;
            Function func = null;
            var temp = first.Invoke(context);
            if (temp.ValueType == JSObjectType.NotExist)
            {
                if (context.thisBind == null)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.ReferenceError("Varible not defined.")));
                else
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(First + " not exist.")));
            }
            if (temp.ValueType != JSObjectType.Function && !(temp.ValueType == JSObjectType.Object && temp.oValue is Function))
                throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(first + " is not callable")));
            func = temp.oValue as Function;
            newThisBind = context.objectSource;

            JSObject arguments = new JSObject(true)
                {
                    ValueType = JSObjectType.Object,
                    oValue = new Arguments(),
                    attributes = ObjectAttributes.DontDelete | ObjectAttributes.DontEnum
                };
            var field = arguments.GetField("length", false, true);
            field.assignCallback();
            field.ValueType = JSObjectType.Int;
            if (args == null)
                args = second.Invoke(null).oValue as Statement[];
            field.iValue = args.Length;
            field.attributes = ObjectAttributes.DontEnum;
            for (int i = 0; i < field.iValue; i++)
            {
                var a = Tools.RaiseIfNotExist(args[i].Invoke(context)).Clone() as JSObject;
                arguments.fields[i.ToString()] = a;
                a.attributes |= ObjectAttributes.Argument;
            }
            arguments.prototype = JSObject.GlobalPrototype;
            arguments.fields["callee"] = field = new JSObject();
            field.ValueType = JSObjectType.Function;
            field.oValue = func;
            field.Protect();
            field.attributes = ObjectAttributes.DontEnum;
            return func.Invoke(context, newThisBind, arguments);
        }

        public override string ToString()
        {
            string res = first + "(";
            var args = second.Invoke(null).oValue as Statement[];
            for (int i = 0; i < args.Length; i++)
            {
                res += args[i];
                if (i + 1 < args.Length)
                    res += ", ";
            }
            return res + ")";
        }

        public override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> vars)
        {
            if (first is IOptimizable)
                Parser.Optimize(ref first, depth + 1, vars);
            if (second is IOptimizable)
                Parser.Optimize(ref second, depth + 1, vars);
            args = second.Invoke(null).oValue as Statement[];
            return false;
        }
    }
}