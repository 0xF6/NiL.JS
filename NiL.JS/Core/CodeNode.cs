﻿using System;
using System.Collections.Generic;
using NiL.JS.Core.BaseTypes;

namespace NiL.JS.Core
{
    [Serializable]
    public abstract class CodeNode
    {
        private static readonly CodeNode[] emptyArray = new CodeNode[0];

        public virtual int Position { get; internal set; }
        public virtual int Length { get; internal set; }
        public virtual int EndPosition { get { return Position + Length; } }

        private CodeNode[] childs;
        public virtual CodeNode[] Childs { get { return childs ?? (childs = getChildsImpl() ?? emptyArray); } }

        protected abstract CodeNode[] getChildsImpl();

        internal virtual NiL.JS.Core.JSObject InvokeForAssing(NiL.JS.Core.Context context)
        {
            return raiseInvalidAssignment();
        }

        protected static JSObject raiseInvalidAssignment()
        {
            throw new JSException(new ReferenceError("Invalid left-hand side in assignment."));
        }

        internal abstract JSObject Invoke(Context context);

        /// <summary>
        /// Заставляет объект перестроить своё содержимое для ускорения работы
        /// </summary>
        /// <param name="_this">Ссылка на экземпляр, для которого происходит вызов функции</param>
        /// <param name="depth">Глубина погружения в выражении</param>
        /// <param name="functionDepth">Глубина погружения в функции. Увеличивается при входе в функцию и уменьшается при выходе из нее</param>
        /// <returns>true если были внесены изменения</returns>
        internal virtual bool Optimize(ref CodeNode _this, int depth, int functionDepth, Dictionary<string, VariableDescriptor> variables, bool strict)
        {
            return false;
        }
    }
}
