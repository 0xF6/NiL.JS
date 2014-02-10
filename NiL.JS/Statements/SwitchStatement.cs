﻿using NiL.JS.Core;
using System;
using System.Collections.Generic;

namespace NiL.JS.Statements
{
    internal class SwitchStatement : Statement, IOptimizable
    {
        private class Case
        {
            public int index;
            public Statement statement;
        }

        private FunctionStatement[] functions;
        private int length;
        private readonly Statement[] body;
        private Case[] cases;
        private Statement image;

        public SwitchStatement(Statement[] body)
        {
            this.body = body;
            length = body.Length - 1;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            string code = state.Code;
            int i = index;
            if (!Parser.Validate(code, "switch (", ref i) && !Parser.Validate(code, "switch(", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(code[i])) i++;
            var image = OperatorStatement.Parse(state, ref i).Statement;
            if (code[i] != ')')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \")\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \"{\" at + " + Tools.PositionToTextcord(code, i))));
            do i++; while (char.IsWhiteSpace(code[i]));
            var body = new List<Statement>();
            var funcs = new List<FunctionStatement>();
            var cases = new List<Case>();
            cases.Add(null);
            state.AllowBreak++;
            while (code[i] != '}')
            {
                do
                {
                    if (Parser.Validate(code, "case", i) && Parser.isIdentificatorTerminator(code[i + 4]))
                    {
                        i += 4;
                        while (char.IsWhiteSpace(code[i])) i++;
                        var sample = OperatorStatement.Parse(state, ref i).Statement;
                        if (code[i] != ':')
                            throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \":\" at + " + Tools.PositionToTextcord(code, i))));
                        i++;
                        cases.Add(new Case() { index = body.Count, statement = new Operators.StrictEqual(image, sample) });
                    }
                    else if (Parser.Validate(code, "default", i) && Parser.isIdentificatorTerminator(code[i + 7]))
                    {
                        if (cases[0] != null)
                            throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Duplicate default case in switch at " + Tools.PositionToTextcord(code, i))));
                        i += 7;
                        if (code[i] != ':')
                            throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Expected \":\" at + " + Tools.PositionToTextcord(code, i))));
                        i++;
                        cases[0] = new Case() { index = body.Count, statement = null };
                    }
                    else break;
                    while (char.IsWhiteSpace(code[i]) || (code[i] == ';')) i++;
                } while (true);
                if (cases.Count == 1 && cases[0] == null)
                    throw new JSException(TypeProxy.Proxy(new Core.BaseTypes.SyntaxError("Switch statement must be contain cases. " + Tools.PositionToTextcord(code, index))));
                var t = Parser.Parse(state, ref i, 0);
                if (t == null)
                    continue;
                if (t is FunctionStatement)
                {
                    if (state.strict.Peek())
                        throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    funcs.Add(t as FunctionStatement);
                }
                else
                    body.Add(t);
                while (char.IsWhiteSpace(code[i]) || (code[i] == ';')) i++;
            }
            state.AllowBreak--;
            i++;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new SwitchStatement(body.ToArray())
                {
                    functions = funcs.ToArray(),
                    cases = cases.ToArray(),
                    image = image
                }
            };
        }

        public override JSObject Invoke(Context context)
        {
            if (functions != null)
                throw new InvalidOperationException();
            int i = cases[0] != null ? cases[0].index : length + 1;
            for (int j = 1; j < cases.Length; j++)
            {
                if ((bool)cases[j].statement.Invoke(context))
                {
                    i = cases[j].index;
                    break;
                }
            }
            for (; i <= length; i++)
            {
                body[i].Invoke(context);
                if (context.abort != AbortType.None)
                {
                    if (context.abort == AbortType.Break)
                        context.abort = AbortType.None;
                    return context.abortInfo;
                }
            }
            return null;
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            if (depth < 1)
                throw new InvalidOperationException();
            Parser.Optimize(ref image, varibles);
            for (int i = 0; i < body.Length; i++)
                Parser.Optimize(ref body[i], 1, varibles);
            for (int i = 0; i < functions.Length; i++)
            {
                Statement stat = functions[i];
                Parser.Optimize(ref stat, 1, varibles);
                varibles[functions[i].Name] = stat;
                functions[i] = null;
            }
            functions = null;
            for (int i = 1; i < cases.Length; i++)
                Parser.Optimize(ref cases[i].statement, 2, varibles);
            return false;
        }

        public override string ToString()
        {
            string res = "switch (" + image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            if (functions != null)
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            for (int i = 0; i < body.Length; i++)
            {
                for (int j = 0; j < cases.Length; j++)
                {
                    if (cases[j] != null && cases[j].index == i)
                    {
                        res += "case " + (cases[j].statement as Operators.StrictEqual).Second + ":" + Environment.NewLine;
                    }
                }
                string lc = body[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            return res + "}";
        }
    }
}