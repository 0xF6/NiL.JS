﻿using System;
using System.Collections.Generic;
using System.Linq;
using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class ObjectDefinition : Expression
    {
        private Expression[] values;
        private string[] fieldNames;
        private KeyValuePair<Expression, Expression>[] computedProperties;

        public Expression[] Values { get { return values; } }
        public string[] FieldNames { get { return fieldNames; } }
        public IEnumerable<KeyValuePair<Expression, Expression>> ComputedProperties { get { return computedProperties; } }

        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Object;
            }
        }

        internal override bool ResultInTempContainer
        {
            get { return false; }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return values.Any(x => x.NeedDecompose);
            }
        }

        private ObjectDefinition(Dictionary<string, Expression> fields, KeyValuePair<Expression, Expression>[] computedProperties)
        {
            this.computedProperties = computedProperties;
            this.fieldNames = new string[fields.Count];
            this.values = new Expression[fields.Count];
            int i = 0;
            foreach (var f in fields)
            {
                this.fieldNames[i] = f.Key;
                this.values[i++] = f.Value;
            }
        }

        internal static CodeNode Parse(ParsingState state, ref int index)
        {
            if (state.Code[index] != '{')
                throw new ArgumentException("Invalid JSON definition");
            var flds = new Dictionary<string, Expression>();
            var computedProperties = new List<KeyValuePair<Expression, Expression>>();
            int i = index;
            while (state.Code[i] != '}')
            {
                do
                    i++;
                while (Tools.IsWhiteSpace(state.Code[i]));
                int s = i;
                if (state.Code[i] == '}')
                    break;

                bool getOrSet = Parser.Validate(state.Code, "get", ref i) || Parser.Validate(state.Code, "set", ref i);
                if (getOrSet)
                {
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;
                }
                var asterisk = state.Code[i] == '*';
                if (asterisk)
                {
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));
                }

                if (Parser.Validate(state.Code, "[", ref i))
                {
                    var name = ExpressionTree.Parse(state, ref i, false, false, false, true, false, false);
                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;
                    if (state.Code[i] != ']')
                        ExceptionsHelper.ThrowSyntaxError("Expected ']'", state.Code, i);
                    do
                        i++;
                    while (Tools.IsWhiteSpace(state.Code[i]));

                    CodeNode initializer;
                    if (state.Code[i] == '(')
                    {
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionType.AnonymousGenerator : FunctionType.AnonymousFunction);
                    }
                    else
                    {
                        initializer = ExpressionTree.Parse(state, ref i);
                    }

                    switch (state.Code[s])
                    {
                        case 'g':
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>((Expression)name, new GsPropertyPairExpression((Expression)initializer, null)));
                                break;
                            }
                        case 's':
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>((Expression)name, new GsPropertyPairExpression(null, (Expression)initializer)));
                                break;
                            }
                        default:
                            {
                                computedProperties.Add(new KeyValuePair<Expression, Expression>((Expression)name, (Expression)initializer));
                                break;
                            }
                    }
                }
                else if (getOrSet && state.Code[i] != ':')
                {
                    i = s;
                    var mode = state.Code[i] == 's' ? FunctionType.Setter : FunctionType.Getter;
                    var propertyAccessor = FunctionDefinition.Parse(state, ref i, mode) as FunctionDefinition;
                    var accessorName = propertyAccessor.name;
                    if (!flds.ContainsKey(accessorName))
                    {
                        var propertyPair = new GsPropertyPairExpression
                        (
                            mode == FunctionType.Getter ? propertyAccessor : null,
                            mode == FunctionType.Setter ? propertyAccessor : null
                        );
                        flds.Add(accessorName, propertyPair);
                    }
                    else
                    {
                        var vle = flds[accessorName] as GsPropertyPairExpression;

                        if (vle == null)
                            ExceptionsHelper.ThrowSyntaxError("Try to define " + mode.ToString().ToLowerInvariant() + " for defined field", state.Code, s);

                        do
                        {
                            if (mode == FunctionType.Getter)
                            {
                                if (vle.Getter == null)
                                {
                                    vle.Getter = propertyAccessor;
                                    break;
                                }
                            }
                            else
                            {
                                if (vle.Setter == null)
                                {
                                    vle.Setter = propertyAccessor;
                                    break;
                                }
                            }

                            ExceptionsHelper.ThrowSyntaxError("Try to redefine " + mode.ToString().ToLowerInvariant() + " of " + propertyAccessor.Name, state.Code, s);
                        }
                        while (false);
                    }
                }
                else
                {
                    if (asterisk)
                    {
                        do
                            i++;
                        while (Tools.IsWhiteSpace(state.Code[i]));
                    }

                    i = s;
                    var fieldName = "";
                    if (Parser.ValidateName(state.Code, ref i, false, true, state.strict))
                        fieldName = Tools.Unescape(state.Code.Substring(s, i - s), state.strict);
                    else if (Parser.ValidateValue(state.Code, ref i))
                    {
                        if (state.Code[s] == '-')
                            ExceptionsHelper.Throw(new SyntaxError("Invalid char \"-\" at " + CodeCoordinates.FromTextPosition(state.Code, s, 1)));
                        double d = 0.0;
                        int n = s;
                        if (Tools.ParseNumber(state.Code, ref n, out d))
                            fieldName = Tools.DoubleToString(d);
                        else if (state.Code[s] == '\'' || state.Code[s] == '"')
                            fieldName = Tools.Unescape(state.Code.Substring(s + 1, i - s - 2), state.strict);
                        else if (flds.Count != 0)
                            ExceptionsHelper.Throw((new SyntaxError("Invalid field name at " + CodeCoordinates.FromTextPosition(state.Code, s, i - s))));
                        else
                            return null;
                    }
                    else
                        return null;

                    while (Tools.IsWhiteSpace(state.Code[i]))
                        i++;

                    Expression initializer = null;

                    if (state.Code[i] == '(')
                    {
                        i = s;
                        initializer = FunctionDefinition.Parse(state, ref i, asterisk ? FunctionType.MethodGenerator : FunctionType.Method);
                    }
                    else
                    {
                        if (asterisk)
                        {
                            ExceptionsHelper.ThrowSyntaxError("Unexpected token", state.Code, i);
                        }

                        if (state.Code[i] != ':' && state.Code[i] != ',' && state.Code[i] != '}')
                            ExceptionsHelper.ThrowSyntaxError("Expected ',', ';' or '}'", state.Code, i);

                        Expression aei = null;
                        if (flds.TryGetValue(fieldName, out aei))
                        {
                            if (state.strict ? (!(aei is ConstantDefinition) || (aei as ConstantDefinition).value != JSValue.undefined)
                                             : aei is GsPropertyPairExpression)
                                ExceptionsHelper.ThrowSyntaxError("Try to redefine field \"" + fieldName + "\"", state.Code, s, i - s);
                            if (state.message != null)
                                state.message(MessageLevel.Warning, CodeCoordinates.FromTextPosition(state.Code, i, 0), "Duplicate key \"" + fieldName + "\"");
                        }

                        if (state.Code[i] == ',' || state.Code[i] == '}')
                        {
                            if (!Parser.ValidateName(fieldName, 0))
                                ExceptionsHelper.ThrowSyntaxError("Invalid variable name", state.Code, i);

                            if (state.Code[i] == ',')
                            {
                                do
                                    i++;
                                while (Tools.IsWhiteSpace(state.Code[i]));
                            }

                            initializer = new GetVariableExpression(fieldName, state.scopeDepth);
                        }
                        else
                        {
                            do
                                i++;
                            while (Tools.IsWhiteSpace(state.Code[i]));
                            initializer = (Expression)ExpressionTree.Parse(state, ref i, false, false);
                        }
                    }
                    flds[fieldName] = initializer;
                }
                while (Tools.IsWhiteSpace(state.Code[i]))
                    i++;
                if ((state.Code[i] != ',') && (state.Code[i] != '}'))
                    return null;
            }
            i++;
            var pos = index;
            index = i;
            return new ObjectDefinition(flds, computedProperties.ToArray())
            {
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = JSObject.CreateObject();
            if (fieldNames.Length == 0 && computedProperties.Length == 0)
                return res;
            res.fields = JSObject.getFieldsContainer();
            for (int i = 0; i < fieldNames.Length; i++)
            {
                var val = values[i].Evaluate(context);
                val = val.CloneImpl(false);
                val.attributes = JSValueAttributesInternal.None;
                if (this.fieldNames[i] == "__proto__")
                    res.__proto__ = val.oValue as JSObject;
                else
                    res.fields[this.fieldNames[i]] = val;
            }

            for (var i = 0; i < computedProperties.Length; i++)
            {
                var key = computedProperties[i].Key.Evaluate(context).CloneImpl(false);
                var value = computedProperties[i].Value.Evaluate(context).CloneImpl(false);

                JSValue existedValue;
                Symbol symbolKey = null;
                string stringKey = null;
                if (key.Is<Symbol>())
                {
                    symbolKey = key.As<Symbol>();
                    if (res.symbols == null)
                        res.symbols = new Dictionary<Symbol, JSValue>();

                    if (!res.symbols.TryGetValue(symbolKey, out existedValue))
                        res.symbols[symbolKey] = existedValue = value;
                }
                else
                {
                    stringKey = key.As<string>();
                    if (!res.fields.TryGetValue(stringKey, out existedValue))
                        res.fields[stringKey] = existedValue = value;
                }

                if (existedValue != value)
                {
                    if (existedValue.Is(JSValueType.Property) && value.Is(JSValueType.Property))
                    {
                        var egs = existedValue.As<GsPropertyPair>();
                        var ngs = value.As<GsPropertyPair>();
                        egs.get = ngs.get ?? egs.get;
                        egs.set = ngs.set ?? egs.set;
                    }
                    else
                    {
                        if (key.Is<Symbol>())
                        {
                            res.symbols[symbolKey] = value;
                        }
                        else
                        {
                            res.fields[stringKey] = value;
                        }
                    }
                }
            }
            return res;
        }

        internal protected override bool Build(ref CodeNode _this, int expressionDepth, List<string> scopeVariables, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics stats, Options opts)
        {
            _codeContext = codeContext;

            for (var i = 0; i < values.Length; i++)
            {
                Parser.Build(ref values[i], 2, scopeVariables, variables, codeContext | CodeContext.InExpression, message, stats, opts);
            }

            for (var i = 0; i < computedProperties.Length; i++)
            {
                var key = computedProperties[i].Key;
                Parser.Build(ref key, 2, scopeVariables, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                var value = computedProperties[i].Value;
                Parser.Build(ref value, 2, scopeVariables, variables, codeContext | CodeContext.InExpression, message, stats, opts);

                computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }

            return false;
        }

        internal protected override void Optimize(ref CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionStatistics stats)
        {
            for (var i = Values.Length; i-- > 0;)
            {
                var cn = Values[i] as CodeNode;
                cn.Optimize(ref cn, owner, message, opts, stats);
                Values[i] = cn as Expression;
            }
            for (var i = 0; i < computedProperties.Length; i++)
            {
                var key = computedProperties[i].Key;
                key.Optimize(ref key, owner, message, opts, stats);

                var value = computedProperties[i].Value;
                value.Optimize(ref value, owner, message, opts, stats);

                computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);
            }
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return values;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        protected internal override void Decompose(ref Expression self, IList<CodeNode> result)
        {
            var lastDecomposeIndex = -1;
            var lastComputeDecomposeIndex = -1;
            for (var i = 0; i < values.Length; i++)
            {
                values[i].Decompose(ref values[i], result);
                if (values[i].NeedDecompose)
                {
                    lastDecomposeIndex = i;
                }
            }
            for (var i = 0; i < computedProperties.Length; i++)
            {
                var key = computedProperties[i].Key;
                key.Decompose(ref key, result);

                var value = computedProperties[i].Value;
                value.Decompose(ref value, result);

                if ((key != computedProperties[i].Key)
                    || (value != computedProperties[i].Value))
                    computedProperties[i] = new KeyValuePair<Expression, Expression>(key, value);

                if (computedProperties[i].Value.NeedDecompose
                    || computedProperties[i].Key.NeedDecompose)
                {
                    lastComputeDecomposeIndex = i;
                }
            }

            if (lastComputeDecomposeIndex >= 0)
                lastDecomposeIndex = values.Length;

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                if (!(values[i] is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(values[i], false));
                    values[i] = new ExtractStoredValueExpression(values[i]);
                }
            }

            for (var i = 0; i < lastDecomposeIndex; i++)
            {
                Expression key = null;
                Expression value = null;

                if (!(computedProperties[i].Key is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(computedProperties[i].Key, false));
                    key = new ExtractStoredValueExpression(computedProperties[i].Key);
                }
                if (!(computedProperties[i].Value is ExtractStoredValueExpression))
                {
                    result.Add(new StoreValueStatement(computedProperties[i].Value, false));
                    value = new ExtractStoredValueExpression(computedProperties[i].Value);
                }
                if ((key != null)
                    || (value != null))
                    computedProperties[i] = new KeyValuePair<Expression, Expression>(
                        key ?? computedProperties[i].Key,
                        value ?? computedProperties[i].Value);
            }
        }

        public override string ToString()
        {
            string res = "{ ";
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if ((values[i] is ConstantDefinition) && ((values[i] as ConstantDefinition).value.valueType == JSValueType.Property))
                {
                    var gs = (values[i] as ConstantDefinition).value.oValue as CodeNode[];
                    res += gs[0];
                    if (gs[0] != null && gs[1] != null)
                        res += ", ";
                    res += gs[1];
                }
                else
                    res += "\"" + fieldNames[i] + "\"" + " : " + values[i];
                if (i + 1 < fieldNames.Length)
                    res += ", ";
            }
            return res + " }";
        }
    }
}