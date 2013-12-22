﻿using NiL.JS.Core.BaseTypes;
using System;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    internal sealed class ForStatement : Statement, IOptimizable
    {
        private Statement init;
        private Statement condition;
        private Statement post;
        private Statement body;
        private int implId;

        private ForStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "for(", ref i) && (!Parser.Validate(code, "for (", ref i)))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            Statement init = null;
            init = code[i] == ';' ? null as Statement : Parser.Parse(state, ref i, 3);
            if (code[i] != ';')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var condition = code[i] == ';' ? null as Statement : OperatorStatement.Parse(state, ref i).Statement;
            if (code[i] != ';')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            var post = code[i] == ')' ? null as Statement : OperatorStatement.Parse(state, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            if (code[i] != ')')
                throw new ArgumentException("code (" + i + ")");
            do i++; while (char.IsWhiteSpace(code[i]));
            state.AllowBreak++;
            state.AllowContinue++;
            var body = Parser.Parse(state, ref i, 0);
            state.AllowBreak--;
            state.AllowContinue--;
            index = i;
            int id = 0;
            if (body != null)
            {
                if (condition == null)
                {
                    if (post == null)
                        id = 0;
                    else
                        id = 1;
                }
                else
                {
                    if (post == null)
                        id = 2;
                    else
                        id = 3;
                }
            }
            else
            {
                id = 4;
            }
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new ForStatement()
                {
                    body = body,
                    condition = condition,
                    init = init,
                    post = post,
                    implId = id
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        private void impl0(Context context)
        {
            for (; ; )
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Continue)
                        context.abort = AbortType.None;
                    else
                    {
                        if (context.abort == AbortType.Break)
                            context.abort = AbortType.None;
                        return;
                    }
                }
            }
        }

        private void impl1(Context context)
        {
            for (; ; )
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Continue)
                        context.abort = AbortType.None;
                    else
                    {
                        if (context.abort == AbortType.Break)
                            context.abort = AbortType.None;
                        return;
                    }
                }
                post.Invoke(context);
            }
        }

        private void impl2(Context context)
        {
            while ((bool)condition.Invoke(context))
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Continue)
                        context.abort = AbortType.None;
                    else
                    {
                        if (context.abort == AbortType.Break)
                            context.abort = AbortType.None;
                        return;
                    }
                }
            }
        }

        private void impl3(Context context)
        {
            while ((bool)condition.Invoke(context))
            {
                body.Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Continue)
                        context.abort = AbortType.None;
                    else
                    {
                        if (context.abort == AbortType.Break)
                            context.abort = AbortType.None;
                        return;
                    }
                }
                post.Invoke(context);
            }
        }

        private void impl4(Context context)
        {
            while ((bool)condition.Invoke(context))
                post.Invoke(context);
        }

        public override JSObject Invoke(Context context)
        {
            if (init != null)
                init.Invoke(context);
            if (implId == 0)
                impl0(context);
            else if (implId == 1)
                impl1(context);
            else if (implId == 2)
                impl2(context);
            else if (implId == 3)
                impl3(context);
            else
                impl4(context);
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref init, depth, varibles);
            Parser.Optimize(ref condition, 1, varibles);
            Parser.Optimize(ref post, 1, varibles);
            Parser.Optimize(ref body, depth + 1, varibles);
            return false;
        }
    }
}