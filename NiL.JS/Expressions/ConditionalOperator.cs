﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ConditionalOperator : Expression
    {
        private Expression[] threads;

        protected internal override bool ContextIndependent
        {
            get
            {
                return base.ContextIndependent
                    && (threads[0] == null || threads[0].ContextIndependent)
                    && (threads[1] == null || threads[1].ContextIndependent);
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                var ftt = threads[0].ResultType;
                var stt = threads[1].ResultType;
                if (ftt == stt)
                    return ftt;
                if (Tools.IsEqual(ftt, stt, PredictedType.Group))
                    return ftt & PredictedType.Group;
                return PredictedType.Ambiguous;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        public IList<Expression> Threads { get { return new ReadOnlyCollection<Expression>(threads); } }

        public ConditionalOperator(Expression first, Expression[] threads)
            : base(first, null, false)
        {
            this.threads = threads;
        }

        public override JSValue Evaluate(Context context)
        {
            return (bool)first.Evaluate(context) ? threads[0].Evaluate(context) : threads[1].Evaluate(context);
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref first, expressionDepth + 1, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref threads[0], expressionDepth, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref threads[1], expressionDepth, variables, codeContext | CodeContext.Conditional | CodeContext.InExpression, message, stats, opts);

            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && expressionDepth <= 1)
            {
                if (threads[0] == null && threads[1] == null)
                {
                    if (first.ContextIndependent)
                    {
                        _this = null;
                        return false;
                    }
                    else
                    {
                        _this = new CommaOperator(first, new ConstantDefinition(JSValue.undefined));
                    }
                }
                else if (threads[0] == null)
                {
                    _this = new LogicalDisjunctionOperator(first, threads[1]) { Position = Position, Length = Length };
                    return true;
                }
                else if (threads[1] == null)
                {
                    _this = new LogicalConjunctionOperator(first, threads[0]) { Position = Position, Length = Length };
                    return true;
                }
                else if (first.ContextIndependent)
                {
                    _this = ((bool)first.Evaluate(null) ? threads[0] : threads[1]);
                    return false;
                }
            }

            base.Build(ref _this, expressionDepth + 1, variables, codeContext, message, stats, opts);
            return false;
        }

        public override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            base.Optimize(ref _this, owner, message, opts, stats);
            for (var i = threads.Length; i-- > 0;)
            {
                var cn = threads[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                threads[i] = cn as Expression;
            }
            if (message != null
                && (threads[0] is GetVariableExpression || threads[0] is ConstantDefinition)
                && (threads[1] is GetVariableExpression || threads[1] is ConstantDefinition)
                && ResultType == PredictedType.Ambiguous)
                message(MessageLevel.Warning, new CodeCoordinates(0, Position, Length), "Type of an expression is ambiguous");
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            base.RebuildScope(functionInfo, transferedVariables, scopeBias);

            threads[0]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
            threads[1]?.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}