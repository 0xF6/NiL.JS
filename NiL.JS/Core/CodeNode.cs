﻿using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;
using System.Linq.Expressions;
using NiL.JS.Core.JIT;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyArray = new CodeNode[0];

        internal virtual Expression BuildTree(TreeBuildingState state)
        {
#if DEBUG
            System.Diagnostics.Debug.Print("JIT for " + this.GetType() + " not implemented");
#endif
            return defaultJit(state);
        }

        internal System.Linq.Expressions.Expression defaultJit(TreeBuildingState state)
        {
            var wraper = System.Linq.Expressions.Expression.Call(
                System.Linq.Expressions.Expression.Constant(this),
                this.GetType().GetMethod("Evaluate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(Context) }, null),
                JITHelpers.ContextParameter
                );
            if (state.AllowReturn)
                return System.Linq.Expressions.Expression.Block(
                        wraper,
                        Expression.IfThen(Expression.Equal(JITHelpers.wrap(AbortType.Return), Expression.Field(JITHelpers.ContextParameter, "abort")),
                                            Expression.Return(JITHelpers.ReturnTarget, Expression.Field(JITHelpers.ContextParameter, "abortInfo"))),
                        JITHelpers.UndefinedConstant
                       );
            else
                return System.Linq.Expressions.Expression.Block(
                        wraper,
                        JITHelpers.UndefinedConstant
                       );
        }

        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public virtual CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyArray); } }

        protected abstract CodeNode[] getChildsImpl();

        internal virtual NiL.JS.Core.JSObject EvaluateForAssing(NiL.JS.Core.Context context)
        {
            return raiseInvalidAssignment();
        }

        protected static JSObject raiseInvalidAssignment()
        {
            throw new JSException(new ReferenceError("Invalid left-hand side in assignment."));
        }

        internal abstract JSObject Evaluate(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое для ускорения работы
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <param name="functionDepth">Глубина погружения в функции. Увеличивается при входе в функцию и уменьшается при выходе из нее</param>
        /// <returns>true если были внесены изменения</returns>
        internal virtual bool Optimize(ref CodeNode _this, int depth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            return false;
        }
    }
}
