﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class InfinityLoop : CodeNode
    {
        private CodeNode body;
        private string[] labels;

        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        internal InfinityLoop(CodeNode body, string[] labels)
        {
            this.body = body ?? new EmptyStatement();
            this.labels = labels;
        }

        internal override JSObject Evaluate(Context context)
        {
            for (; ; )
            {
#if DEV
                if (context.debugging && !(body is CodeBlock))
                    context.raiseDebugger(body);
#endif
                context.lastResult = body.Evaluate(context) ?? context.lastResult;
                if (context.abort != AbortType.None)
                {
                    var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                    var _break = (context.abort > AbortType.Continue) || !me;
                    if (context.abort < AbortType.Return && me)
                    {
                        context.abort = AbortType.None;
                        context.abortInfo = null;
                    }
                    if (_break)
                        return null;
                }
            }
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(NiL.JS.Core.JIT.TreeBuildingState state)
        {
            var continueTarget = System.Linq.Expressions.Expression.Label("continue" + (DateTime.Now.Ticks % 1000));
            var breakTarget = System.Linq.Expressions.Expression.Label("break" + (DateTime.Now.Ticks % 1000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueTarget;
            state.BreakLabels.Push(breakTarget);
            state.ContinueLabels.Push(continueTarget);
            try
            {
                return System.Linq.Expressions.Expression.Loop(body.CompileToIL(state), breakTarget, continueTarget);
            }
            finally
            {
                if (state.BreakLabels.Peek() != breakTarget)
                    throw new InvalidOperationException();
                state.BreakLabels.Pop();
                if (state.ContinueLabels.Peek() != continueTarget)
                    throw new InvalidOperationException();
                state.ContinueLabels.Pop();
                for (var i = 0; i < labels.Length; i++)
                    state.NamedContinueLabels.Remove(labels[i]);
            }
        }

#endif

        protected override CodeNode[] getChildsImpl()
        {
            return new[] { body };
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            return false;
        }

        internal override void Optimize(ref CodeNode _this, Expressions.FunctionExpression owner, CompilerMessageCallback message)
        {
            body.Optimize(ref body, owner, message);
        }

        public override string ToString()
        {
            return "for (;;)" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}