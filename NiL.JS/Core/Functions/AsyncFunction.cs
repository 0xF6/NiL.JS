﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.BaseLibrary;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using NiL.JS.Extensions;

namespace NiL.JS.Core.Functions
{
    [Prototype(typeof(Function), true)]
    internal sealed class AsyncFunction : Function
    {
        private sealed class Сontinuator
        {
            private readonly AsyncFunction _asyncFunction;
            private readonly JSValue _promise;
            private readonly Task<JSValue> _task;
            private readonly Context _context;

            private JSValue _result;

            public JSValue ResultPromise { get; private set; }

            public Сontinuator(JSValue promise, AsyncFunction asyncFunction, Context context)
            {
                _promise = promise;
                _asyncFunction = asyncFunction;
                _context = context;
                _task = new Task<JSValue>(() => _result);
            }

            public void Build()
            {
                ResultPromise = Marshal(new Promise(_task));
                build(_promise);
            }

            private void build(JSValue promise)
            {
                var thenFunction = promise["then"];
                if (thenFunction == null || thenFunction.ValueType != JSValueType.Function)
                    throw new JSException(new TypeError("The promise has no function \"then\""));

                thenFunction.As<Function>().Call(promise, new Arguments { new Func<JSValue, JSValue>(then), new Func<JSValue, JSValue>(fail) });
            }

            private JSValue fail(JSValue arg)
            {
                throw new NotImplementedException();
            }

            private JSValue then(JSValue arg)
            {
                _context._executionInfo = arg;
                _context._executionMode = ExecutionMode.Resume;

                var result = _asyncFunction.run(_context);

                if (_context._executionMode == ExecutionMode.Suspend)
                {
                    build(_context._executionInfo);
                }
                else
                {
                    _result = result;
                    _task.Start();
                }

                return null;
            }
        }

        public override JSValue prototype
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public AsyncFunction(Context context, FunctionDefinition implementation)
            : base(context, implementation)
        {
            RequireNewKeywordLevel = RequireNewKeywordLevel.WithoutNewOnly;
        }

        protected internal override JSValue Invoke(bool construct, JSValue targetObject, Arguments arguments)
        {
            if (construct)
                ExceptionHelper.ThrowTypeError("Async function cannot be invoked as a constructor");

            var body = _functionDefinition._body;
            if (body._lines.Length == 0)
            {
                notExists._valueType = JSValueType.NotExists;
                return notExists;
            }

            if (arguments == null)
                arguments = new Arguments(Context.CurrentContext);

            var internalContext = new Context(_initialContext, true, this);
            internalContext._definedVariables = Body._variables;

            initContext(targetObject, arguments, true, internalContext);
            initParameters(arguments, internalContext);

            var result = run(internalContext);

            result = processSuspend(internalContext, result);

            return result;
        }

        private JSValue processSuspend(Context internalContext, JSValue result)
        {
            if (internalContext._executionMode == ExecutionMode.Suspend)
            {
                var promise = internalContext._executionInfo;
                var continuator = new Сontinuator(promise, this, internalContext);
                continuator.Build();
                result = continuator.ResultPromise;
            }
            else
            {
                result = Marshal(Promise.resolve(result));
            }

            return result;
        }

        private JSValue run(Context internalContext)
        {
            internalContext.Activate();
            JSValue result = null;
            try
            {
                result = evaluateBody(internalContext);
            }
            finally
            {
                internalContext.Deactivate();
            }

            return result;
        }
    }
}
