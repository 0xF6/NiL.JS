﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;

namespace NiL.JS.Core
{
    public enum PredictedType
    {
        Unknown = 0x0,
        Ambiguous = 0x10,
        Undefined = 0x20,
        Bool = 0x30,
        Number = 0x40,
        Int = 0x41,
        Double = 0x42,
        String = 0x50,
        Object = 0x60,
        Function = 0x61,
        Class = 0x62,
        Group = 0xF0,
        Full = 0xFF
    }

#if !PORTABLE
    [Serializable]
#endif
    public class VariableDescriptor
    {
        internal int definitionScopeLevel;
        internal Context cacheContext;
        internal JSValue cacheRes;
        internal readonly string name;
        internal bool captured;
        internal bool lexicalScope;
        internal Expression initializer;
        internal List<Expression> assignments;
        internal readonly List<VariableReference> references;
        internal CodeNode owner;
        internal PredictedType lastPredictedType;
        internal bool isReadOnly;
        internal bool isDefined;
        internal int scopeBias;

        public bool IsDefined { get { return isDefined; } }
        public CodeNode Owner { get { return owner; } }
        public bool IsReadOnly { get { return isReadOnly; } }
        public Expression Initializer { get { return initializer; } }
        public string Name { get { return name; } }
        public int ReferenceCount { get { return references.Count; } }
        public bool LexicalScope { get { return lexicalScope; } }
        public ReadOnlyCollection<Expression> Assignments { get { return assignments == null ? null : assignments.AsReadOnly(); } }

        public IEnumerable<VariableReference> References
        {
            get
            {
                for (var i = 0; i < references.Count; i++)
                    yield return references[i];
            }
        }

        internal JSValue Get(Context context, bool forWrite, int depth)
        {
            context.objectSource = null;
            if (((definitionScopeLevel | depth) & int.MinValue) != 0)
                return context.GetVariable(name, forWrite);
            if (context == cacheContext)
                return cacheRes;
            return deepGet(context, forWrite, depth);
        }

        private JSValue deepGet(Context context, bool forWrite, int depth)
        {
            TypeProxy tp = null;
            JSValue res = null;
            var defsl = depth - definitionScopeLevel;
            while (defsl > 0)
            {
                defsl--;
                context = context.parent;
            }
            if (context != cacheContext)
                cacheRes = null;
            else if (cacheRes != null)
                return cacheRes;
            if (lexicalScope)
            {
                if (context.fields == null || !context.fields.TryGetValue(name, out res))
                    return JSValue.NotExists;
            }
            else
            {
                res = context.GetVariable(name, forWrite);
                if ((res.attributes & JSValueAttributesInternal.SystemObject) != 0)
                    return res;
                if (forWrite
                    && !IsDefined
                    && res.valueType == JSValueType.NotExists)
                    res.attributes = JSValueAttributesInternal.None;
                else
                {
                    tp = res.oValue as TypeProxy;
                    if (tp != null)
                        res = tp.prototypeInstance ?? res;
                }
            }
            cacheContext = context;
            cacheRes = res;
            return res;
        }

        internal VariableDescriptor(string name, int definitionDepth)
        {
            this.isDefined = true;
            this.definitionScopeLevel = definitionDepth;
            this.name = name;
            this.references = new List<VariableReference>();
        }

        internal VariableDescriptor(VariableReference proto, int definitionDepth)
        {
            this.definitionScopeLevel = definitionDepth;
            this.name = proto.Name;
            if (proto is EntityReference)
                initializer = (proto as EntityReference).Entity;
            this.references = new List<VariableReference>() { proto };
            proto._descriptor = this;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
