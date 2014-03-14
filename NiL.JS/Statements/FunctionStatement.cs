﻿using NiL.JS.Core.BaseTypes;
using System;
using System.Collections.Generic;
using NiL.JS.Core;

namespace NiL.JS.Statements
{
    public sealed class FunctionStatement : Statement
    {
        public enum FunctionParseMode
        {
            function = 0,
            get,
            set
        }

        private string[] argumentsNames;
        private CodeBlock body;
        internal readonly string name;
        internal FunctionParseMode mode;

        public CodeBlock Body { get { return body; } }
        public string Name { get { return name; } }

        private FunctionStatement(string name)
        {
            this.name = name;
        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            return Parse(state, ref index, FunctionParseMode.function);
        }

        internal static ParseResult Parse(ParsingState state, ref int index, FunctionParseMode mode)
        {
            string code = state.Code;
            int i = index;
            switch (mode)
            {
                case FunctionParseMode.function:
                    {
                        if (!Parser.Validate(code, "function", ref i))
                            return new ParseResult();
                        if ((code[i] != '(') && (!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
                case FunctionParseMode.get:
                    {
                        if (!Parser.Validate(code, "get", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
                case FunctionParseMode.set:
                    {
                        if (!Parser.Validate(code, "set", ref i))
                            return new ParseResult();
                        if ((!char.IsWhiteSpace(code[i])))
                            return new ParseResult() { IsParsed = false, Message = "Invalid char in function definition" };
                        break;
                    }
            }
            bool inExp = state.InExpression;
            state.InExpression = false;
            while (char.IsWhiteSpace(code[i])) i++;
            var arguments = new List<string>();
            string name = null;
            if (code[i] != '(')
            {
                int n = i;
                if (!Parser.ValidateName(code, ref i, true, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid function parameters definition " + (string.IsNullOrEmpty(name) ? "for function \"" + name + "\" " : "") + "at " + Tools.PositionToTextcord(code, n))));
                name = Tools.Unescape(code.Substring(n, i - n));
                while (char.IsWhiteSpace(code[i])) i++;
                if (code[i] != '(')
                    throw new ArgumentException("Invalid char at " + i + ": '" + code[i] + "'");
            }
            else if (mode != FunctionParseMode.function)
                throw new ArgumentException("Getters and Setters must have name");
            do i++; while (char.IsWhiteSpace(code[i]));
            if (code[i] == ',')
                throw new ArgumentException("code (" + i + ")");
            while (code[i] != ')')
            {
                if (code[i] == ',')
                    do i++; while (char.IsWhiteSpace(code[i]));
                int n = i;
                if (!Parser.ValidateName(code, ref i, true, state.strict.Peek()))
                    throw new JSException(TypeProxy.Proxy(new SyntaxError("Invalid description of function arguments.")));
                arguments.Add(Tools.Unescape(code.Substring(n, i - n)));
                while (char.IsWhiteSpace(code[i])) i++;
            }
            switch (mode)
            {
                case FunctionParseMode.get:
                    {
                        if (arguments.Count != 0)
                            throw new ArgumentException("getter have many arguments");
                        break;
                    }
                case FunctionParseMode.set:
                    {
                        if (arguments.Count != 1)
                            throw new ArgumentException("setter have invalid arguments");
                        break;
                    }
            }
            do
                i++;
            while (char.IsWhiteSpace(code[i]));
            if (code[i] != '{')
                throw new ArgumentException("code (" + i + ")");
            var labels = state.Labels;
            state.Labels = new List<string>();
            state.AllowReturn++;
            CodeBlock body = null;
            try
            {
                state.AllowStrict = true;
                body = CodeBlock.Parse(state, ref i).Statement as CodeBlock;
            }
            finally
            {
                state.AllowStrict = false;
                state.Labels = labels;
                state.AllowReturn--;
            }
            if (!inExp)
            {
                var tindex = i;
                while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i])) i++;
                if (i < code.Length && code[i] == '(')
                {
                    List<Statement> args = new List<Statement>();
                    i++;
                    for (; ; )
                    {
                        while (char.IsWhiteSpace(code[i])) i++;
                        if (code[i] == ')')
                            break;
                        else if (code[i] == ',')
                            do i++; while (char.IsWhiteSpace(code[i]));
                        args.Add(OperatorStatement.Parse(state, ref i, false).Statement);
                    }
                    i++;
                    index = i;
                    while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
                    if (i < code.Length && code[i] == ';')
                        throw new JSException(TypeProxy.Proxy(new SyntaxError("Expression can not start with word \"function\"")));
                    return new ParseResult()
                    {
                        IsParsed = true,
                        Message = "",
                        Statement = new Operators.Call(new FunctionStatement(name)
                        {
                            argumentsNames = arguments.ToArray(),
                            body = body
                        },
                        new ImmidateValueStatement(args.ToArray()))
                    };
                }
                else
                    i = tindex;
            }
            state.InExpression = inExp;
            index = i;
            FunctionStatement res = new FunctionStatement(name)
                {
                    argumentsNames = arguments.ToArray(),
                    body = body,
                    mode = mode
                };
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = res
            };
        }

        internal override JSObject Invoke(Context context)
        {
            Function res = new Function(context, body, argumentsNames, name);
            return res;
        }

        internal override bool Optimize(ref Statement _this, int depth, System.Collections.Generic.Dictionary<string, Statement> varibles)
        {
            var stat = body as Statement;
            body.Optimize(ref stat, 0, varibles);
            body = stat as CodeBlock;
            return false;
        }

        public override string ToString()
        {
            var res = mode + " " + name + "(";
            if (argumentsNames != null)
                for (int i = 0; i < argumentsNames.Length; )
                    res += argumentsNames[i] + (++i < argumentsNames.Length ? "," : "");
            res += ")" + ((object)body ?? "{ [native code] }").ToString();
            return res;
        }
    }
}