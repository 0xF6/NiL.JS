﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class WithStatement : CodeNode
    {
        private CodeNode scope;
        private CodeNode body;

        public CodeNode Body { get { return body; } }
        public CodeNode Scope { get { return scope; } }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "with (", ref i) && !Parser.Validate(state.Code, "with(", ref i))
                return null;
            if (state.strict)
                ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("WithStatement is not allowed in strict mode.")));

            if (state.message != null)
                state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, index, 4), "Do not use \"with\".");

            var obj = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("Invalid syntax WithStatement.")));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));

            CodeNode body = null;
            state.scopeDepth++;
            try
            {
                body = Parser.Parse(state, ref i, 0);
            }
            finally
            {
                state.scopeDepth--;
            }

            if (body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            var pos = index;
            index = i;
            return new WithStatement()
                {
                    scope = obj,
                    body = body,
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            JSValue scopeObject = null;
            WithContext intcontext = null;
            Action<Context> action = null;

            if (context.abortType >= AbortType.Resume)
            {
                action = context.SuspendData[this] as Action<Context>;
                if (action != null)
                {
                    action(context);
                    return null;
                }
            }

#if DEV
            if (context.abortType != AbortType.Resume && context.debugging)
                context.raiseDebugger(scope);
#endif
            scopeObject = scope.Evaluate(context);
            if (context.abortType == AbortType.Suspend)
            {
                context.SuspendData[this] = null;
                return null;
            }

            intcontext = new WithContext(scopeObject, context);
#if DEV
            if (context.debugging && !(body is CodeBlock))
                context.raiseDebugger(body);
#endif
            action = (c) =>
            {
                try
                {
                    intcontext.abortType = c.abortType;
                    intcontext.abortInfo = c.abortInfo;
                    intcontext.Activate();
                    c.lastResult = body.Evaluate(intcontext) ?? intcontext.lastResult;
                    c.abortType = intcontext.abortType;
                    c.abortInfo = intcontext.abortInfo;
                    if (c.abortType == AbortType.Suspend)
                    {
                        c.SuspendData[this] = action;
                    }
                }
                finally
                {
                    intcontext.Deactivate();
                }
            };
            action(context);
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                scope
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            if (stats != null)
                stats.ContainsWith = true;
            Parser.Build(ref scope, expressionDepth + 1, scopeVariables, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            Parser.Build(ref body, expressionDepth, scopeVariables, variables, codeContext | CodeContext.InWith, message, stats, opts);
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            if (scope != null)
                scope.Optimize(ref scope, owner, message, opts, stats);
            if (body != null)
                body.Optimize(ref body, owner, message, opts, stats);
        }

        internal protected override void Decompose(ref CodeNode self)
        {
            if (scope != null)
                scope.Decompose(ref scope);
            if (body != null)
                body.Decompose(ref body);
        }

        public override string ToString()
        {
            return "with (" + scope + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
