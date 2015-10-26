﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class GetMemberOperator : Expression
    {
        private JSValue cachedMemberName;

        public CodeNode Source { get { return first; } }
        public CodeNode FieldName { get { return second; } }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        internal GetMemberOperator(Expression obj, Expression fieldName)
            : base(obj, fieldName, false)
        {
        }

        internal protected override JSValue EvaluateForWrite(Context context)
        {
            JSValue res = null;
            JSValue source = null;
            source = first.Evaluate(context);
            if (source.valueType < JSValueType.Object)
                source = source.Clone() as JSValue;
            else
                source = source.oValue as JSValue ?? source;
            res = source.GetMember(cachedMemberName ?? second.Evaluate(context), true, MemberScope.Сommon);
            context.objectSource = source;
            if (res.valueType == JSValueType.NotExists)
                res.valueType = JSValueType.NotExistsInObject;
            return res;
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue res = null;
            JSValue source = null;
            source = first.Evaluate(context);
            if (source.valueType < JSValueType.Object)
                source = source.CloneImpl(false);
            else if (source != source.oValue)
            {
                res = source.oValue as JSValue;
                if (res != null)
                {
                    source = res;
                    res = null;
                }
            }
            res = source.GetMember(cachedMemberName ?? second.Evaluate(context), false, MemberScope.Сommon);
            context.objectSource = source;
            if (res.valueType == JSValueType.NotExists)
                res.valueType = JSValueType.NotExistsInObject;
            else if (res.valueType == JSValueType.Property)
                res = Tools.InvokeGetter(res, source);
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext state, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (statistic != null)
                statistic.UseGetMember = true;
            base.Build(ref _this, depth, variables, state, message, statistic, opts);
            if (second is ConstantDefinition)
            {
                cachedMemberName = second.Evaluate(null);
                if (statistic != null && cachedMemberName.ToString() == "arguments")
                    statistic.ContainsArguments = true;
            }
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
            if (second is ConstantDefinition
                && (second as ConstantDefinition).value.ToString().Length > 0
                && (Parser.ValidateName((second as ConstantDefinition).value.ToString(), ref i, true)))
                res += "." + (second as ConstantDefinition).value;
            else
                res += "[" + second + "]";
            return res;
        }
    }
}