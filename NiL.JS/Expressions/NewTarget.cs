﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    public sealed class NewTarget : Expression
    {
        protected internal override bool ContextIndependent
        {
            get
            {
                return false;
            }
        }

        protected internal override bool NeedDecompose
        {
            get
            {
                return false;
            }
        }

        protected internal override bool LValueModifier
        {
            get
            {
                return false;
            }
        }

        public NewTarget()
        {

        }

        public override JSValue Evaluate(Context context)
        {
            if (context.thisBind != null && (context.thisBind.attributes & JSValueAttributesInternal.ConstructingObject) != 0)
            {
                while (context.oldContext != null && context.oldContext.thisBind == context.thisBind)
                {
                    context = context.oldContext;
                }

                return context.owner;
            }

            return JSValue.undefined;
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            return false;
        }

        public override void Optimize(ref Core.CodeNode _this, FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {

        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return "new.target";
        }
    }
}
