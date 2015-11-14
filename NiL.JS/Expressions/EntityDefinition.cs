﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class EntityReference : VariableReference
    {
        private EntityDefinition owner;

        public EntityDefinition Entity { get { return owner; } }

        public override string Name
        {
            get { return owner.name; }
        }

        public override JSValue Evaluate(Context context)
        {
            return owner.Evaluate(context);
        }

        public EntityReference(EntityDefinition owner)
        {
            defineFunctionDepth = -1;
            this.owner = owner;
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return owner.ToString();
        }
    }

    /// <summary>
    /// Базовый тип для ClassNotation и FunctionNotation.
    /// 
    /// Base type fot ClassNotation and FunctionNotation.
    /// </summary>
    public abstract class EntityDefinition : Expression
    {
        internal VariableReference reference;
        internal string name;

        public string Name { get { return name; } }
        public VariableReference Reference { get { return reference; } }

        public abstract bool Hoist { get; }

        protected EntityDefinition()
        {
            reference = new EntityReference(this);
        }

        internal protected override bool Build(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionStatistics statistic, Options opts)
        {
            _codeContext = codeContext;
            return false;
        }

        internal protected virtual void Register(Dictionary<string, VariableDescriptor> variables, CodeContext codeContext)
        {
            if ((codeContext & CodeContext.InExpression) == 0 && name != null) // имя не задано только для случая Function("<some string>")
            {
                VariableDescriptor desc = null;
                if (!variables.TryGetValue(name, out desc) || desc == null)
                    variables[name] = Reference.descriptor ?? new VariableDescriptor(Reference, true, Reference.defineFunctionDepth);
                else
                {
                    variables[name] = Reference.descriptor;
                    for (var j = 0; j < desc.references.Count; j++)
                        desc.references[j].descriptor = Reference.descriptor;
                    Reference.descriptor.references.AddRange(desc.references);
                    Reference.descriptor.captured = Reference.descriptor.captured || Reference.descriptor.references.FindIndex(x => x.defineFunctionDepth > x.descriptor.defineDepth) != -1;
                }
            }
        }

        protected internal override abstract void Decompose(ref Expression self, IList<CodeNode> result);
    }
}
