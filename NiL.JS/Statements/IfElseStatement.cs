﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class IfStatement : CodeNode
    {
        private Expression condition;
        private CodeNode then;

        public CodeNode Then { get { return then; } }
        public CodeNode Condition { get { return condition; } }

        internal IfStatement(Expression condition, CodeNode body)
        {
            this.condition = condition;
            this.then = body;
        }

        public override JSValue Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(then is CodeBlock))
                    context.raiseDebugger(then);
#endif
                var temp = then.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
            }
            return null;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return new[] { then, condition };
        }

        public override string ToString()
        {
            string rp = Environment.NewLine;
            string rs = Environment.NewLine + "  ";
            var sbody = then.ToString();
            return "if (" + condition + ")" + (then is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs));
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var cc = condition as CodeNode;
            condition.Optimize(ref cc, owner, message, opts, statistic);
            condition = (Expression)cc;
            if (then != null)
                then.Optimize(ref then, owner, message, opts, statistic);
            else
                _this = condition;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class IfElseStatement : CodeNode
    {
        private Expression condition;
        private CodeNode then;
        private CodeNode @else;

        public CodeNode Then { get { return then; } }
        public CodeNode Else { get { return @else; } }
        public Expression Condition { get { return condition; } }

        private IfElseStatement()
        {

        }

        public IfElseStatement(Expression condition, CodeNode body, CodeNode elseBody)
        {
            this.condition = condition;
            this.then = body;
            this.@else = elseBody;
        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "if (", ref i) && !Parser.Validate(state.Code, "if(", ref i))
                return null;
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            var condition = (Expression)ExpressionTree.Parse(state, ref i);
            while (char.IsWhiteSpace(state.Code[i]))
                i++;
            if (state.Code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do
                i++;
            while (char.IsWhiteSpace(state.Code[i]));
            CodeNode body = Parser.Parse(state, ref i, 0);
            if (body is FunctionDefinition)
            {
                if (state.strict)
                    ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, body.Position, body.Length), "Do not declare function in nested blocks.");
                body = new CodeBlock(new[] { body }); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            CodeNode elseBody = null;
            var pos = i;
            while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]))
                i++;
            if (i < state.Code.Length && !(body is CodeBlock) && (state.Code[i] == ';'))
                do
                    i++;
                while (i < state.Code.Length && char.IsWhiteSpace(state.Code[i]));
            if (Parser.Validate(state.Code, "else", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i]))
                    i++;
                elseBody = Parser.Parse(state, ref i, 0);
                if (elseBody is FunctionDefinition)
                {
                    if (state.strict)
                        ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    if (state.message != null)
                        state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, elseBody.Position, elseBody.Length), "Do not declare function in nested blocks.");
                    elseBody = new CodeBlock(new[] { elseBody }); // для того, чтобы не дублировать код по декларации функции, 
                    // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
                }
            }
            else
                i = pos;
            pos = index;
            index = i;
            return new IfElseStatement()
                {
                    then = body,
                    condition = condition,
                    @else = elseBody,
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
#if DEV
            if (context.debugging)
                context.raiseDebugger(condition);
#endif
            if ((bool)condition.Evaluate(context))
            {
#if DEV
                if (context.debugging && !(then is CodeBlock))
                    context.raiseDebugger(then);
#endif
                var temp = then.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
                return null;
            }
            else
            {
#if DEV
                if (context.debugging && !(@else is CodeBlock))
                    context.raiseDebugger(@else);
#endif
                var temp = @else.Evaluate(context);
                if (temp != null)
                    context.lastResult = temp;
                return null;
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                then,
                condition,
                @else
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            Parser.Build(ref condition, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            Parser.Build(ref then, depth, variables, codeContext | CodeContext.Conditional, message, statistic, opts);
            Parser.Build(ref @else, depth, variables, codeContext | CodeContext.Conditional, message, statistic, opts);

            if ((opts & Options.SuppressUselessExpressionsElimination) == 0 && condition is ToBooleanOperator)
            {
                if (message != null)
                    message(MessageLevel.Warning, new CodeCoordinates(0, condition.Position, 2), "Useless conversion. Remove double negation in condition");
                condition = (condition as Expression).first;
            }
            try
            {
                if ((opts & Options.SuppressUselessExpressionsElimination) == 0
                    && (condition is ConstantDefinition || (condition.ContextIndependent)))
                {
                    if ((bool)condition.Evaluate(null))
                        _this = then;
                    else
                        _this = @else;
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
            if (_this == this && @else == null)
                _this = new IfStatement(this.condition, this.then) { Position = Position, Length = Length };
            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            var cc = condition as CodeNode;
            condition.Optimize(ref cc, owner, message, opts, statistic);
            condition = (Expression)cc;
            if (then != null)
                then.Optimize(ref then, owner, message, opts, statistic);
            if (@else != null)
                @else.Optimize(ref @else, owner, message, opts, statistic);
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string rp = Environment.NewLine;
            string rs = Environment.NewLine + "  ";
            var sbody = then.ToString();
            var sebody = @else == null ? "" : @else.ToString();
            return "if (" + condition + ")" +
                (then is CodeBlock ? sbody : Environment.NewLine + "  " + sbody.Replace(rp, rs)) +
                (@else != null ? Environment.NewLine +
                "else" + Environment.NewLine +
                (@else is CodeBlock ? sebody.Replace(rp, rs) : "  " + sebody) : "");
        }
    }
}