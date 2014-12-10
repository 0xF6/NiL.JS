﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NiL.JS.Core;
using NiL.JS.Core.JIT;
using NiL.JS.Expressions;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Statements
{
    [Serializable]
    public sealed class ForOfStatement : CodeNode
    {
        private CodeNode variable;
        private CodeNode source;
        private CodeNode body;
        private string[] labels;

        public CodeNode Variable { get { return variable; } }
        public CodeNode Source { get { return source; } }
        public CodeNode Body { get { return body; } }
        public ReadOnlyCollection<string> Labels { get { return System.Array.AsReadOnly<string>(labels); } }

        private ForOfStatement()
        {

        }

        internal static ParseResult Parse(ParsingState state, ref int index)
        {
            //string code = state.Code;
            int i = index;
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (!Parser.Validate(state.Code, "for(", ref i) && (!Parser.Validate(state.Code, "for (", ref i)))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            var res = new ForOfStatement()
            {
                labels = state.Labels.GetRange(state.Labels.Count - state.LabelCount, state.LabelCount).ToArray()
            };
            var vStart = i;
            if (Parser.Validate(state.Code, "var", ref i))
            {
                while (char.IsWhiteSpace(state.Code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                    throw new JSException(new SyntaxError("Invalid variable name at " + CodeCoordinates.FromTextPosition(state.Code, start)));
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (varName == "arguments" || varName == "eval")
                        throw new JSException((new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start))));
                }
                res.variable = new VariableDefineStatement(varName, new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, functionDepth = state.functionsDepth }, false, state.functionsDepth) { Position = vStart, Length = i - vStart };
            }
            else
            {
                if (state.Code[i] == ';')
                    return new ParseResult();
                while (char.IsWhiteSpace(state.Code[i])) i++;
                int start = i;
                string varName;
                if (!Parser.ValidateName(state.Code, ref i, state.strict.Peek()))
                    return new ParseResult();
                varName = Tools.Unescape(state.Code.Substring(start, i - start), state.strict.Peek());
                if (state.strict.Peek())
                {
                    if (varName == "arguments" || varName == "eval")
                        throw new JSException((new Core.BaseTypes.SyntaxError("Parameters name may not be \"arguments\" or \"eval\" in strict mode at " + CodeCoordinates.FromTextPosition(state.Code, start))));
                }
                res.variable = new GetVariableExpression(varName, state.functionsDepth) { Position = start, Length = i - start, functionDepth = state.functionsDepth };
            }
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] == '=')
            {
                do i++; while (char.IsWhiteSpace(state.Code[i]));
                var defVal = ExpressionTree.Parse(state, ref i, true, false, false, true, false, true);
                if (!defVal.IsParsed)
                    return defVal;
                NiL.JS.Expressions.Expression exp = new OpAssignCache(res.variable as GetVariableExpression ?? (res.variable as VariableDefineStatement).initializators[0] as GetVariableExpression);
                exp = new Assign(
                    exp,
                    (NiL.JS.Expressions.Expression)defVal.Statement)
                    {
                        Position = res.variable.Position,
                        Length = defVal.Statement.EndPosition - res.variable.Position
                    };
                if (res.variable == exp.first.first)
                    res.variable = exp;
                else
                    (res.variable as VariableDefineStatement).initializators[0] = exp;
                while (char.IsWhiteSpace(state.Code[i])) i++;
            }
            if (!Parser.Validate(state.Code, "of", ref i))
                return new ParseResult();
            while (char.IsWhiteSpace(state.Code[i])) i++;
            res.source = Parser.Parse(state, ref i, 1);
            while (char.IsWhiteSpace(state.Code[i])) i++;
            if (state.Code[i] != ')')
                throw new JSException((new Core.BaseTypes.SyntaxError("Expected \")\" at + " + CodeCoordinates.FromTextPosition(state.Code, i))));
            i++;
            state.AllowBreak.Push(true);
            state.AllowContinue.Push(true);
            res.body = Parser.Parse(state, ref i, 0);
            if (res.body is FunctionExpression)
            {
                if (state.strict.Peek())
                    throw new JSException((new NiL.JS.Core.BaseTypes.SyntaxError("In strict mode code, functions can only be declared at top level or immediately within another function.")));
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, res.body.Position), "Do not declare function in nested blocks.");
                res.body = new CodeBlock(new[] { res.body }, state.strict.Peek()); // для того, чтобы не дублировать код по декларации функции, 
                // она оборачивается в блок, который сделает самовыпил на втором этапе, но перед этим корректно объявит функцию.
            }
            state.AllowBreak.Pop();
            state.AllowContinue.Pop();
            res.Position = index;
            res.Length = i - index;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = res
            };
        }

#if !NET35

        internal override System.Linq.Expressions.Expression CompileToIL(Core.JIT.TreeBuildingState state)
        {
            var continueTarget = System.Linq.Expressions.Expression.Label("continue" + (DateTime.Now.Ticks % 10000));
            var nextProto = System.Linq.Expressions.Expression.Label();
            var breakTarget = System.Linq.Expressions.Expression.Label("break" + (DateTime.Now.Ticks % 10000));
            for (var i = 0; i < labels.Length; i++)
                state.NamedContinueLabels[labels[i]] = continueTarget;
            state.BreakLabels.Push(breakTarget);
            state.ContinueLabels.Push(continueTarget);
            try
            {
                var val = System.Linq.Expressions.Expression.Parameter(typeof(JSObject));
                var @enum = System.Linq.Expressions.Expression.Parameter(typeof(NiL.JS.Core.Tools._ForcedEnumerator<string>));
                var source = System.Linq.Expressions.Expression.Parameter(typeof(JSObject));
                var res = System.Linq.Expressions.Expression.Block(new[] { val, @enum, source },
                    System.Linq.Expressions.Expression.Assign(source, this.source.CompileToIL(state)),
                    System.Linq.Expressions.Expression.Assign(val, System.Linq.Expressions.Expression.Call(JITHelpers.ContextParameter, typeof(Context).GetMethod("GetVariable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(string), typeof(bool) }, null), JITHelpers.wrap(variable.ToString()), JITHelpers.wrap(true))),
                    System.Linq.Expressions.Expression.Loop(
                            System.Linq.Expressions.Expression.IfThenElse(
                                    System.Linq.Expressions.Expression.AndAlso(System.Linq.Expressions.Expression.ReferenceNotEqual(source, JITHelpers.wrap(null)),
                                        System.Linq.Expressions.Expression.AndAlso(System.Linq.Expressions.Expression.Property(source, "isDefinded"),
                                                        System.Linq.Expressions.Expression.OrElse(System.Linq.Expressions.Expression.LessThan(System.Linq.Expressions.Expression.Convert(System.Linq.Expressions.Expression.Field(source, "valueType"), typeof(int)), JITHelpers.wrap((int)JSObjectType.Object)),
                                                                       System.Linq.Expressions.Expression.ReferenceNotEqual(System.Linq.Expressions.Expression.Field(source, "oValue"), JITHelpers.wrap(null))))),
                                      System.Linq.Expressions.Expression.Block(
                                                System.Linq.Expressions.Expression.Assign(@enum, System.Linq.Expressions.Expression.Call(JITHelpers.methodof(new Func<JSObject, object>(NiL.JS.Core.Tools._ForcedEnumerator<string>.create)), source))
                                                , System.Linq.Expressions.Expression.Loop(
                                                    System.Linq.Expressions.Expression.IfThenElse(System.Linq.Expressions.Expression.Call(@enum, typeof(NiL.JS.Core.Tools._ForcedEnumerator<string>).GetMethod("MoveNext")), System.Linq.Expressions.Expression.Block(
                                                        System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(val, "valueType"), JITHelpers.wrap(JSObjectType.String))
                                                        , System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(val, "oValue"), System.Linq.Expressions.Expression.Property(@enum, "Current"))
                                                        , System.Linq.Expressions.Expression.Call(val, typeof(JSObject).GetMethod("Assign"), System.Linq.Expressions.Expression.Call(source, typeof(JSObject).GetMethod("GetMember", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(JSObject), typeof(bool), typeof(bool) }, null), val, JITHelpers.wrap(false), JITHelpers.wrap(false)))
                                                        , this.body.CompileToIL(state)
                                                        , System.Linq.Expressions.Expression.Label(continueTarget)
                                                    ), System.Linq.Expressions.Expression.Goto(nextProto)))
                                                , System.Linq.Expressions.Expression.Label(nextProto)
                                                , System.Linq.Expressions.Expression.IfThenElse(System.Linq.Expressions.Expression.ReferenceNotEqual(System.Linq.Expressions.Expression.Field(source, "__proto__"), JITHelpers.wrap(null)),
                                                    System.Linq.Expressions.Expression.Assign(source, System.Linq.Expressions.Expression.Field(source, "__proto__"))
                                                , System.Linq.Expressions.Expression.Block(
                                                    System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(val, "valueType"), JITHelpers.wrap(JSObjectType.String))
                                                    , System.Linq.Expressions.Expression.Assign(System.Linq.Expressions.Expression.Field(val, "oValue"), JITHelpers.wrap("__proto__"))
                                                    , System.Linq.Expressions.Expression.Assign(source, System.Linq.Expressions.Expression.Call(source, typeof(JSObject).GetMethod("GetMember", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new Type[] { typeof(JSObject), typeof(bool), typeof(bool) }, null), val, JITHelpers.wrap(false), JITHelpers.wrap(false)))
                                                ))
                                            )
                                      , System.Linq.Expressions.Expression.Goto(breakTarget)))
                    , System.Linq.Expressions.Expression.Label(breakTarget)
                    );
                return res;
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

        internal override JSObject Evaluate(Context context)
        {
            var s = source.Evaluate(context);
            JSObject v = null;
            if (variable is Assign)
            {
                variable.Evaluate(context);
                v = (variable as Assign).first.Evaluate(context);
            }
            else
                v = variable.EvaluateForAssing(context);
            if (!s.IsDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
                return JSObject.undefined;
            int index = 0;
            HashSet<string> processedKeys = new HashSet<string>(StringComparer.Ordinal);
            while (s != null)
            {
                if (s.oValue is Core.BaseTypes.Array)
                {
                    var src = s.oValue as Core.BaseTypes.Array;
                    foreach (var item in src.data)
                    {
                        if (item == null
                            || !item.IsExist
                            || (item.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                            continue;
                        v.Assign(item);
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
                                context.abortInfo = JSObject.notExists;
                            }
                            if (_break)
                                return null;
                        }
                    }
                    if (src.fields != null)
                        foreach (var item in src.fields)
                        {
                            if (item.Value == null
                                || !item.Value.IsExist
                                || (item.Value.attributes & JSObjectAttributesInternal.DoNotEnum) != 0)
                                continue;
                            if (processedKeys.Contains(item.Key))
                                continue;
                            processedKeys.Add(item.Key);
                            v.Assign(item.Value);
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
                                    context.abortInfo = JSObject.notExists;
                                }
                                if (_break)
                                    return null;
                            }
                        }
                }
                else
                {
                    var keys = s.GetEnumerator();
                    for (; ; )
                    {
                        try
                        {
                            if (!keys.MoveNext())
                                break;
                        }
                        catch (InvalidOperationException)
                        {
                            keys = s.GetEnumerator();
                            for (int i = 0; i < index && keys.MoveNext(); i++) ;
                        }
                        if (processedKeys.Contains(keys.Current))
                            continue;
                        processedKeys.Add(keys.Current);
                        v.valueType = JSObjectType.String;
                        v.oValue = keys.Current;
                        v.Assign(s.GetMember(v, false, true));
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
                                context.abortInfo = JSObject.notExists;
                            }
                            if (_break)
                                return null;
                        }
                        index++;
                    }
                }
                s = s.__proto__;
                if (s == JSObject.Null || !s.IsDefinded || (s.valueType >= JSObjectType.Object && s.oValue == null))
                    break;
            }
            return null;
        }

        protected override CodeNode[] getChildsImpl()
        {
            var res = new List<CodeNode>()
            {
                body,
                variable,
                source
            };
            res.RemoveAll(x => x == null);
            return res.ToArray();
        }

        internal override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict, CompilerMessageCallback message)
        {
            Parser.Build(ref variable, 2, variables, strict, message);
            var tvar = variable as VariableDefineStatement;
            if (tvar != null)
                variable = tvar.initializators[0];
            if (variable is Assign)
                ((variable as Assign).first.first as GetVariableExpression).forceThrow = false;
            Parser.Build(ref source, 2, variables, strict, message);
            Parser.Build(ref body, System.Math.Max(1, depth), variables, strict, message);
            if (variable is Expressions.None)
            {
                if ((variable as Expressions.None).SecondOperand != null)
                    throw new InvalidOperationException("Invalid left-hand side in for-in");
                variable = (variable as Expressions.None).FirstOperand;
            }
            if (message != null
                && (source is Json
                || source is ArrayStatement
                || source is Constant))
                message(MessageLevel.Recomendation, new CodeCoordinates(0, Position), "for..in with constant source. This reduce performance. Rewrite without using for..in.");
            return false;
        }

        internal override void Optimize(ref CodeNode _this, FunctionExpression owner, CompilerMessageCallback message)
        {
            source.Optimize(ref source, owner, message);
            if (body != null)
                body.Optimize(ref body, owner, message);
        }

        public override string ToString()
        {
            return "for (" + variable + " in " + source + ")" + (body is CodeBlock ? "" : Environment.NewLine + "  ") + body;
        }
    }
}