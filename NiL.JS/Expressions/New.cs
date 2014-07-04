﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class New : Expression
    {
        private static readonly JSObject newMarker = new JSObject() { valueType = JSObjectType.Object, oValue = typeof(New) };

        private sealed class ThisSetter : CodeNode
        {
            private CodeNode source;
            public JSObject lastThisBind;

            public ThisSetter(CodeNode source)
            {
                this.source = source;
            }

            protected override CodeNode[] getChildsImpl()
            {
                throw new InvalidOperationException();
            }

            internal override JSObject Invoke(Context context)
            {
                JSObject ctor = source.Invoke(context);
                if (ctor.valueType != JSObjectType.Function && !(ctor.valueType == JSObjectType.Object && ctor.oValue is Function))
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(ctor + " is not callable")));
                if (ctor.oValue is MethodProxy)
                    throw new JSException(TypeProxy.Proxy(new NiL.JS.Core.BaseTypes.TypeError(ctor + " can't be used as a constructor")));
                if (ctor.oValue is EvalFunction
                    || ctor.oValue is ExternalFunction
                    || ctor.oValue is MethodProxy)
                    throw new JSException(new TypeError("Function \"" + (ctor.oValue as Function).name + "\" is not a constructor."));

                JSObject _this = null;
                if (!(ctor.oValue is ProxyConstructor))
                {
                    _this = new JSObject(true) { valueType = JSObjectType.Object };
                    _this.__proto__ = ctor.GetMember("prototype");
                    if (_this.__proto__.valueType < JSObjectType.Object)
                        _this.__proto__ = null;
                    else
                        _this.__proto__ = _this.__proto__.Clone() as JSObject;
                    _this.oValue = _this;
                }
                else
                    _this = newMarker;
                context.objectSource = _this;
                lastThisBind = _this;
                return ctor;
            }

            public override string ToString()
            {
                return source.ToString();
            }

            internal override bool Optimize(ref CodeNode _this, int depth, int functionDepth, Dictionary<string, VariableDescriptor> variables, bool strict)
            {
                return source.Optimize(ref source, depth, functionDepth, variables, strict);
            }
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        private ThisSetter thisSetter;

        public New(CodeNode first, CodeNode[] arguments)
            : base(null, null, false)
        {
            if (first is Call)
                this.first = new Call(thisSetter = new ThisSetter((first as Call).FirstOperand), (first as Call).Arguments);
            else
                this.first = new Call(thisSetter = new ThisSetter(first), arguments);
        }

        internal override NiL.JS.Core.JSObject Invoke(NiL.JS.Core.Context context)
        {
            var prevTB = thisSetter.lastThisBind;
            try
            {
                thisSetter.lastThisBind = null;
                var temp = first.Invoke(context);
                if (temp.valueType >= JSObjectType.Object && temp.oValue != null)
                    return temp;
                return thisSetter.lastThisBind;
            }
            finally
            {
                thisSetter.lastThisBind = prevTB;
            }
        }

        public override string ToString()
        {
            return "new " + first.ToString();
        }
    }
}