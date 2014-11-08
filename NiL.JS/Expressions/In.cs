﻿using System;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class In : Expression
    {
        public In(CodeNode first, CodeNode second)
            : base(first, second, true)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            tempContainer.Assign(first.Evaluate(context));
            var source = second.Evaluate(context);
            if (source.valueType < JSObjectType.Object)
                throw new JSException(new TypeError("Right-hand value of operator in is not object."));
            if (tempContainer.valueType == JSObjectType.Int)
            {
                var array = source.oValue as Core.BaseTypes.Array;
                if (array != null)
                    return tempContainer.iValue >= 0 && tempContainer.iValue < array.data.Length && (array.data[tempContainer.iValue] ?? JSObject.notExists).isExist;
            }
            var t = source.GetMember(tempContainer.ToString());
            return t.isExist ? NiL.JS.Core.BaseTypes.Boolean.True : NiL.JS.Core.BaseTypes.Boolean.False;
        }

        public override string ToString()
        {
            return "(" + first + " in " + second + ")";
        }
    }
}