﻿using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class SwitchCase
    {
        internal int index;
        internal CodeNode statement;

        public int Index { get { return index; } }
        public CodeNode Statement { get { return statement; } }
    }

#if !PORTABLE
    [Serializable]
#endif
    public sealed class SwitchStatement : CodeNode
    {
        private sealed class SuspendData
        {
            public JSValue imageValue;
            public int caseIndex;
            public int lineIndex;
        }

        private FunctionDefinition[] functions;
        private CodeNode[] lines;
        private SwitchCase[] cases;
        private CodeNode image;

        public FunctionDefinition[] Functions { get { return functions; } }
        public CodeNode[] Body { get { return lines; } }
        public SwitchCase[] Cases { get { return cases; } }
        public CodeNode Image { get { return image; } }

        internal SwitchStatement(CodeNode[] body)
        {
            this.lines = body;
        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            int i = index;
            if (!Parser.Validate(state.Code, "switch (", ref i) && !Parser.Validate(state.Code, "switch(", ref i))
                return null;
            while (Tools.IsWhiteSpace(state.Code[i]))
                i++;
            var image = ExpressionTree.Parse(state, ref i);
            if (state.Code[i] != ')')
                ExceptionsHelper.Throw((new SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));
            if (state.Code[i] != '{')
                ExceptionsHelper.Throw((new SyntaxError("Expected \"{\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
            do
                i++;
            while (Tools.IsWhiteSpace(state.Code[i]));
            var body = new List<CodeNode>();
            var funcs = new List<FunctionDefinition>();
            var cases = new List<SwitchCase>();
            cases.Add(new SwitchCase() { index = int.MaxValue });
            state.AllowBreak.Push(true);
            while (state.Code[i] != '}')
            {
                do
                {
                    if (Parser.Validate(state.Code, "case", i) && Parser.IsIdentificatorTerminator(state.Code[i + 4]))
                    {
                        i += 4;
                        while (Tools.IsWhiteSpace(state.Code[i]))
                            i++;
                        var sample = ExpressionTree.Parse(state, ref i);
                        if (state.Code[i] != ':')
                            ExceptionsHelper.Throw((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        i++;
                        cases.Add(new SwitchCase() { index = body.Count, statement = sample });
                    }
                    else if (Parser.Validate(state.Code, "default", i) && Parser.IsIdentificatorTerminator(state.Code[i + 7]))
                    {
                        i += 7;
                        while (Tools.IsWhiteSpace(state.Code[i]))
                            i++;
                        if (cases[0].index != int.MaxValue)
                            ExceptionsHelper.Throw((new SyntaxError("Duplicate default case in switch at " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        if (state.Code[i] != ':')
                            ExceptionsHelper.Throw((new SyntaxError("Expected \":\" at + " + CodeCoordinates.FromTextPosition(state.Code, i, 0))));
                        i++;
                        cases[0].index = body.Count;
                    }
                    else
                        break;
                    while (Tools.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';'))
                        i++;
                } while (true);
                if (cases.Count == 1 && cases[0].index == int.MaxValue)
                    ExceptionsHelper.Throw((new SyntaxError("Switch statement must contain cases. " + CodeCoordinates.FromTextPosition(state.Code, index, 0))));
                var t = Parser.Parse(state, ref i, 0);
                if (t == null)
                    continue;
                if (t is FunctionDefinition)
                {
                    if (state.strict)
                        ExceptionsHelper.Throw((new NiL.JS.BaseLibrary.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                    funcs.Add(t as FunctionDefinition);
                }
                else
                    body.Add(t);
                while (Tools.IsWhiteSpace(state.Code[i]) || (state.Code[i] == ';'))
                    i++;
            }
            state.AllowBreak.Pop();
            i++;
            var pos = index;
            index = i;
            return new SwitchStatement(body.ToArray())
                {
                    functions = funcs.ToArray(),
                    cases = cases.ToArray(),
                    image = image,
                    Position = pos,
                    Length = index - pos
                };
        }

        public override JSValue Evaluate(Context context)
        {
#if DEBUG
            if (functions != null)
                throw new InvalidOperationException();
#endif
            JSValue imageValue = null;
            int caseIndex = 1;
            int lineIndex = cases[0].index;

            if (context.abortType >= AbortType.Resume)
            {
                var sdata = context.SuspendData[this] as SuspendData;
                if (sdata.imageValue == null)
                    imageValue = image.Evaluate(context);
                else
                    imageValue = sdata.imageValue;
                caseIndex = sdata.caseIndex;
                lineIndex = sdata.lineIndex;
            }
            else
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(image);
#endif
                imageValue = image.Evaluate(context);
            }
            if (context.abortType == AbortType.Suspend)
            {
                context.SuspendData[this] = new SuspendData() { caseIndex = 1 };
                return null;
            }

            for (; caseIndex < cases.Length; caseIndex++)
            {
#if DEV
                if (context.debugging)
                    context.raiseDebugger(cases[caseIndex].statement);
#endif
                var cseResult = cases[caseIndex].statement.Evaluate(context);
                if (context.abortType == AbortType.Suspend)
                {
                    context.SuspendData[this] = new SuspendData()
                    {
                        caseIndex = caseIndex,
                        imageValue = imageValue
                    };
                    return null;
                }

                if (Expressions.StrictEqualOperator.Check(imageValue, cseResult))
                {
                    lineIndex = cases[caseIndex].index;
                    caseIndex = cases.Length;
                    break;
                }
            }
            for (; lineIndex < lines.Length; lineIndex++)
            {
                if (lines[lineIndex] == null)
                    continue;

                context.lastResult = lines[lineIndex].Evaluate(context) ?? context.lastResult;
                if (context.abortType != AbortType.None)
                {
                    if (context.abortType == AbortType.Break)
                    {
                        context.abortType = AbortType.None;
                    }
                    else if (context.abortType == AbortType.Suspend)
                    {
                        context.SuspendData[this] = new SuspendData()
                        {
                            caseIndex = caseIndex,
                            imageValue = imageValue,
                            lineIndex = lineIndex
                        };
                    }
                    return null;
                }
            }
            return null;
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            if (depth < 1)
                throw new InvalidOperationException();
            Parser.Build(ref image, 2, variables, codeContext | CodeContext.InExpression, message, statistic, opts);
            for (int i = 0; i < lines.Length; i++)
                Parser.Build(ref lines[i], 1, variables, codeContext | CodeContext.Conditional, message, statistic, opts);
            for (int i = 0; functions != null && i < functions.Length; i++)
            {
                CodeNode stat = functions[i];
                Parser.Build(ref stat, 1, variables, codeContext, message, statistic, opts);

                functions[i].Register(variables, codeContext);
            }
            functions = null;
            for (int i = 1; i < cases.Length; i++)
                Parser.Build(ref cases[i].statement, 2, variables, codeContext, message, statistic, opts);
            return false;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                image
            };
            res.AddRange(lines);
            if (functions != null && functions.Length > 0)
                res.AddRange(functions);
            if (cases.Length > 0)
                res.AddRange(from c in cases where c != null select c.statement);
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal protected override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics statistic)
        {
            image.Optimize(ref image, owner, message, opts, statistic);
            for (var i = 1; i < cases.Length; i++)
                cases[i].statement.Optimize(ref cases[i].statement, owner, message, opts, statistic);
            for (var i = lines.Length; i-- > 0; )
            {
                if (lines[i] == null)
                    continue;
                var cn = lines[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, statistic);
                lines[i] = cn;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            string res = "switch (" + image + ") {" + Environment.NewLine;
            var replp = Environment.NewLine;
            var replt = Environment.NewLine + "  ";
            for (int i = lines.Length; i-- > 0; )
            {
                for (int j = 0; j < cases.Length; j++)
                {
                    if (cases[j] != null && cases[j].index == i)
                    {
                        res += "case " + cases[j].statement + ":" + Environment.NewLine;
                    }
                }
                string lc = lines[i].ToString().Replace(replp, replt);
                res += "  " + lc + (lc[lc.Length - 1] != '}' ? ";" + Environment.NewLine : Environment.NewLine);
            }
            if (functions != null)
            {
                for (var i = 0; i < functions.Length; i++)
                {
                    var func = functions[i].ToString().Replace(replp, replt);
                    res += "  " + func + Environment.NewLine;
                }
            }
            return res + "}";
        }

        protected internal override void Decompose(ref CodeNode self)
        {
            for (var i = 0; i < cases.Length; i++)
            {
                if (cases[i].statement != null)
                {
                    cases[i].statement.Decompose(ref cases[i].statement);
                }
            }

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i].Decompose(ref lines[i]);
            }
        }
    }
}