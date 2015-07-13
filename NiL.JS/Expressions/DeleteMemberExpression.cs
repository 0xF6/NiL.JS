﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class DeleteMemberExpression : Expression
    {
        private JSValue cachedMemberName;

        public Expression Source { get { return first; } }
        public Expression FieldName { get { return second; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal DeleteMemberExpression(Expression obj, Expression fieldName)
            : base(obj, fieldName, true)
        {
            if (fieldName is ConstantNotation)
                cachedMemberName = fieldName.Evaluate(null);
        }

        internal override JSValue Evaluate(Context context)
        {
            JSValue source = null;
            source = first.Evaluate(context);
            if (source.valueType < JSValueType.Object)
                source = source.Clone() as JSValue;
            else
                source = source.oValue as JSValue ?? source;
            var res = source.DeleteMember(cachedMemberName ?? second.Evaluate(context));
            context.objectSource = null;
            if (!res && context.strict)
                throw new JSException(new TypeError("Can not delete property \"" + first + "\"."));
            return res;
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, _BuildState state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            return false;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            var res = first.ToString();
            int i = 0;
            if (second is ConstantNotation
                && (second as ConstantNotation).value.ToString().Length > 0
                && (Parser.ValidateName((second as ConstantNotation).value.ToString(), ref i, true)))
                res += "." + (second as ConstantNotation).value;
            else
                res += "[" + second.ToString() + "]";
            return "delete " + res;
        }
    }
}