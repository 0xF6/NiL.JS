﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core;
using NiL.JS.BaseLibrary;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class WhileStatement : CodeNode
    {
        private bool allowRemove;
        private CodeNode condition;
        private CodeNode body;
        private string[] labels;

        public CodeNode Condition { get { return condition; } }
        public CodeNode Body { get { return body; } }
        public ICollection<string> Labels { get { return new ReadOnlyCollection<string>(labels); } }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            if (!Parser.Validate(state.Code, "while (", ref i) && !Parser.Validate(state.Code, "while(", ref i))
                return null;
            int labelsCount = state.LabelCount;
            state.LabelCount = 0;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var condition = Parser.Parse(state, ref i, CodeFragmentType.Expression);
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
                i++;
            if (i >= state.Code.Length)
                ExceptionsHelper.Throw(new SyntaxError("Unexpected end of line."));
            if (state.Code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do
                i++;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
            if (i >= state.Code.Length)
                ExceptionsHelper.Throw(new SyntaxError("Unexpected end of line."));
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            int ccs = state.continiesCount;
            int cbs = state.breaksCount;
            var body = Parser.Parse(state, ref i, 0);
            if (body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            var pos = index;
            index = i;
            return new WhileStatement()
                {
                    allowRemove = ccs == state.continiesCount && cbs == state.breaksCount,
                    body = body,
                    condition = condition,
                    labels = state.Labels.GetRange(state.Labels.Count - labelsCount, labelsCount).ToArray(),
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
            bool be = body != null;
            JSValue checkResult;

            if (context.abortType != AbortType.Resume || context.SuspendData[this] == condition)
            {
#if DEV
                if (context.abortType != AbortType.Resume && context.debugging)
                    context.raiseDebugger(condition);
#endif
                checkResult = condition.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = condition;
                    return null;
                }
                if (!(bool)checkResult)
                    return null;
            }

            do
            {
                if (be
                 && (context.abortType != AbortType.Resume
                    || context.SuspendData[this] == body))
                {
#if DEV
                    if (context.abortType != AbortType.Resume && context.debugging && !(body is CodeBlock))
                        context.raiseDebugger(body);
#endif
                    var temp = body.Evaluate(context);
                    if (temp != null)
                        context.lastResult = temp;
                    if (context.abortType != AbortType.None)
                    {
                        if (context.abortType < AbortType.Return)
                        {
                            var me = context.abortInfo == null || System.Array.IndexOf(labels, context.abortInfo.oValue as string) != -1;
                            var _break = (context.abortType > AbortType.Continue) || !me;
                            if (me)
                            {
                                context.abortType = AbortType.None;
                                context.abortInfo = null;
                            }
                            if (_break)
                                return null;
                        }
                        else if (context.abortType == AbortType.Suspend)
                        {
                            context.SuspendData[this] = body;
                            return null;
                        }
                        else
                            return null;
                    }
                }

#if DEV
                if (context.abortType != AbortType.Resume && context.debugging)
                    context.raiseDebugger(condition);
#endif
                checkResult = condition.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = condition;
                    return null;
                }
            }
            while ((bool)checkResult);
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                condition
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            depth = System.Math.Max(1, depth);
            Parser.Build(ref body, depth, variables, codeContext | CodeContext.Conditional | CodeContext.InLoop, message, statistic, opts);
            Parser.Build(ref condition, 2, variables, codeContext | CodeContext.InLoop | CodeContext.InExpression, message, statistic, opts);
            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && condition is ToBooleanOperator)
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, condition.Position, 2), "Useless conversion. Remove double negation in condition");
                condition = (condition as Expression).first;
            }
            try
            {
                if (allowRemove && (condition is ConstantDefinition || (condition is Expression && (condition as Expression).ContextIndependent)))
                {
                    Eliminated = true;
                    if ((bool)condition.Evaluate(null))
                    {
                        if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && body != null)
                            _this = new InfinityLoopStatement(body, labels);
                    }
                    else if ((opts & Options.SuppressUselessStatementsElimination) == 0)
                    {
                        _this = null;
                        if (body != null)
                            body.Eliminated = true;
                    }
                    condition.Eliminated = true;
                }
                else if ((opts & Options.SuppressUselessExpressionsElimination) == 0
                        && ((condition is ObjectDefinition && (condition as ObjectDefinition).FieldNames.Length == 0)
                            || (condition is ArrayDefinition && (condition as ArrayDefinition).Elements.Count == 0)))
                {
                    _this = new InfinityLoopStatement(body, labels);
                    condition.Eliminated = true;
                }
            }
#if PORTABLE
            catch
            {
#else
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(10, "Error", e.Message);
#endif
            }
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            if (condition != null)
                condition.Optimize(ref condition, owner, message, opts, statistic);
            if (body != null)
                body.Optimize(ref body, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "while (" + condition + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }

        internal protected override void Decompose(ref CodeNode self)
        {
            if (condition != null)
                condition.Decompose(ref condition);
            if (body != null)
                body.Decompose(ref body);
        }
    }
}