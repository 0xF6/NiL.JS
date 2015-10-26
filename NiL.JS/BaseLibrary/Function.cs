﻿using System;
using System.Collections.Generic;
using System.Text;
using NiL.JS.Core;
using NiL.JS.Core.Functions;
using NiL.JS.Core.Interop;
using NiL.JS.Expressions;
using NiL.JS.Statements;

namespace NiL.JS.BaseLibrary
{
    /// <summary>
    /// Возможные типы функции в контексте использования.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public enum FunctionType
    {
        Function = 0,
        Get,
        Set,
        AnonymousFunction,
        Generator,
        Method,
        Arrow
    }

#if !PORTABLE
    [Serializable]
#endif
    public enum RequireNewKeywordLevel
    {
        Both = 0,
        OnlyWithNew,
        OnlyWithoutNew
    }

#if !PORTABLE
    [Serializable]
#endif
    public class Function : JSObject
    {
#if !PORTABLE
        private class _DelegateWraper
        {
            private Function function;

            public _DelegateWraper(Function func)
            {
                function = func;
            }

            public RT Invoke<RT>()
            {
                var eargs = new Arguments();
                eargs.length = 0;
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1>(T1 a1)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new Arguments();
                eargs.length = 3;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new Arguments();
                eargs.length = 4;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new Arguments();
                eargs.length = 5;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new Arguments();
                eargs.length = 6;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new Arguments();
                eargs.length = 7;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new Arguments();
                eargs.length = 8;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new Arguments();
                eargs.length = 9;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new Arguments();
                eargs.length = 10;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new Arguments();
                eargs.length = 11;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new Arguments();
                eargs.length = 12;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new Arguments();
                eargs.length = 13;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new Arguments();
                eargs.length = 14;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new Arguments();
                eargs.length = 15;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                return (RT)function.Invoke(eargs).Value;
            }

            public RT Invoke<RT, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new Arguments();
                eargs.length = 16;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                eargs[15] = TypeProxy.Proxy(a16);
                return (RT)function.Invoke(eargs).Value;
            }

            public void Invoke()
            {
                var eargs = new Arguments();
                eargs.length = 0;
                function.Invoke(eargs);
            }

            public void Invoke<T1>(T1 a1)
            {
                var eargs = new Arguments();


                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2>(T1 a1, T2 a2)
            {
                var eargs = new Arguments();
                eargs.length = 2;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
            {
                var eargs = new Arguments();
                eargs.length = 3;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4)
            {
                var eargs = new Arguments();
                eargs.length = 4;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
            {
                var eargs = new Arguments();
                eargs.length = 5;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6)
            {
                var eargs = new Arguments();
                eargs.length = 6;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7)
            {
                var eargs = new Arguments();
                eargs.length = 7;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8)
            {
                var eargs = new Arguments();
                eargs.length = 8;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9)
            {
                var eargs = new Arguments();
                eargs.length = 9;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10)
            {
                var eargs = new Arguments();
                eargs.length = 10;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11)
            {
                var eargs = new Arguments();
                eargs.length = 11;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12)
            {
                var eargs = new Arguments();
                eargs.length = 12;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13)
            {
                var eargs = new Arguments();
                eargs.length = 13;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14)
            {
                var eargs = new Arguments();
                eargs.length = 14;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15)
            {
                var eargs = new Arguments();
                eargs.length = 15;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                function.Invoke(eargs);
            }

            public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5, T6 a6, T7 a7, T8 a8, T9 a9, T10 a10, T11 a11, T12 a12, T13 a13, T14 a14, T15 a15, T16 a16)
            {
                var eargs = new Arguments();
                eargs.length = 16;
                eargs[0] = TypeProxy.Proxy(a1);
                eargs[1] = TypeProxy.Proxy(a2);
                eargs[2] = TypeProxy.Proxy(a3);
                eargs[3] = TypeProxy.Proxy(a4);
                eargs[4] = TypeProxy.Proxy(a5);
                eargs[5] = TypeProxy.Proxy(a6);
                eargs[6] = TypeProxy.Proxy(a7);
                eargs[7] = TypeProxy.Proxy(a8);
                eargs[8] = TypeProxy.Proxy(a9);
                eargs[9] = TypeProxy.Proxy(a10);
                eargs[10] = TypeProxy.Proxy(a11);
                eargs[11] = TypeProxy.Proxy(a12);
                eargs[12] = TypeProxy.Proxy(a13);
                eargs[13] = TypeProxy.Proxy(a14);
                eargs[14] = TypeProxy.Proxy(a15);
                eargs[15] = TypeProxy.Proxy(a16);
                function.Invoke(eargs);
            }
        }
#endif

        private static readonly FunctionDefinition creatorDummy = new FunctionDefinition("anonymous");
        internal static readonly Function emptyFunction = new Function();
        private static readonly Function TTEProxy = new MethodProxy(typeof(Function)
#if PORTABLE
            .GetTypeInfo().GetDeclaredMethod("ThrowTypeError"))
#else
.GetMethod("ThrowTypeError", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
#endif
            {
                attributes = JSValueAttributesInternal.DoNotDelete
                | JSValueAttributesInternal.Immutable
                | JSValueAttributesInternal.DoNotEnumerate
                | JSValueAttributesInternal.ReadOnly
            };
        protected static void ThrowTypeError()
        {
            ExceptionsHelper.Throw(new TypeError("Properties caller, callee and arguments not allowed in strict mode."));
        }
        internal static readonly JSValue propertiesDummySM = new JSValue()
        {
            valueType = JSValueType.Property,
            oValue = new PropertyPair() { get = TTEProxy, set = TTEProxy },
            attributes = JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.Immutable | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.NonConfigurable
        };

        internal readonly FunctionDefinition creator;
        [Hidden]
        internal readonly Context parentContext;
        [Hidden]
        public Context Context
        {
            [Hidden]
            get { return parentContext; }
        }
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual string name
        {
            [Hidden]
            get { return creator.name; }
        }
        [Hidden]
        internal Number _length = null;
        [Field]
        [ReadOnly]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSValue length
        {
            [Hidden]
            get
            {
                if (_length == null)
                {
                    _length = new Number(0) { attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate };
                    _length.iValue = creator.parameters.Length;
                }
                return _length;
            }
        }
        [Hidden]
        public virtual FunctionType Type
        {
            [Hidden]
            get { return creator.type; }
        }
        [Hidden]
        public virtual bool Strict
        {
            [Hidden]
            get
            {
                return creator.body.strict;
            }
        }
        [Hidden]
        public virtual CodeBlock Body
        {
            [Hidden]
            get
            {
                return creator != null ? creator.body : null;
            }
        }

        [Hidden]
        public virtual RequireNewKeywordLevel RequireNewKeywordLevel
        {
            [Hidden]
            get;
            [Hidden]
            set;
        }

        #region Runtime
        [Hidden]
        internal JSValue _prototype;
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        [NotConfigurable]
        public virtual JSValue prototype
        {
            [Hidden]
            get
            {
                if (_prototype == null)
                {
                    if ((attributes & JSValueAttributesInternal.ProxyPrototype) != 0)
                    {
                        // Вызывается в случае Function.prototype.prototype
                        _prototype = new JSValue(); // выдавать тут константу undefined нельзя, иначе будет падать на вызове defineProperty
                    }
                    else
                    {
                        var res = JSObject.CreateObject(true);
                        res.attributes = JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.NonConfigurable;
                        (res.fields["constructor"] = this.CloneImpl(false)).attributes = JSValueAttributesInternal.DoNotEnumerate;
                        _prototype = res;
                    }
                }
                return _prototype;
            }
            [Hidden]
            set
            {
                _prototype = value.oValue as JSObject ?? value;
            }
        }
        /// <summary>
        /// Объект, содержащий параметры вызова функции либо null если в данный момент функция не выполняется.
        /// </summary>
        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSValue arguments
        {
            [Hidden]
            get
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null)
                    return null;
                if (creator.body.strict)
                    ExceptionsHelper.Throw(new TypeError("Property arguments not allowed in strict mode."));
                if (context.arguments == null && creator.recursionDepth > 0)
                    BuildArgumentsObject();
                return context.arguments;
            }
            [Hidden]
            set
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null)
                    return;
                if (creator.body.strict)
                    ExceptionsHelper.Throw(new TypeError("Property arguments not allowed in strict mode."));
                context.arguments = value;
            }
        }

        [Field]
        [DoNotDelete]
        [DoNotEnumerate]
        public virtual JSValue caller
        {
            [Hidden]
            get
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null || context.oldContext == null)
                    return null;
                if (context.strict || (context.oldContext.strict && context.oldContext.owner != null))
                    ExceptionsHelper.Throw(new TypeError("Property caller not allowed in strict mode."));
                return context.oldContext.owner;
            }
            [Hidden]
            set
            {
                var context = Context.GetRunnedContextFor(this);
                if (context == null || context.oldContext == null)
                    return;
                if (context.strict || (context.oldContext.strict && context.oldContext.owner != null))
                    ExceptionsHelper.Throw(new TypeError("Property caller not allowed in strict mode."));
            }
        }
        #endregion

        [DoNotEnumerate]
        public Function()
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            creator = creatorDummy;
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        [DoNotEnumerate]
        public Function(Arguments args)
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            parentContext = (Context.CurrentContext ?? Context.GlobalContext).Root;
            if (parentContext == Context.globalContext)
                throw new InvalidOperationException("Special Functions constructor can be called only in runtime.");
            var index = 0;
            int len = args.Length - 1;
            var argn = "";
            for (int i = 0; i < len; i++)
                argn += args[i] + (i + 1 < len ? "," : "");
            string code = "function (" + argn + "){" + Environment.NewLine + (len == -1 ? "undefined" : args[len]) + Environment.NewLine + "}";
            var func = FunctionDefinition.Parse(new ParsingState(Tools.RemoveComments(code, 0), code, null), ref index, FunctionType.Method);
            if (func != null && code.Length == index)
            {
                Parser.Build(ref func, 0, new Dictionary<string, VariableDescriptor>(), parentContext.strict ? CodeContext.Strict : CodeContext.None, null, null, Options.Default);
                creator = func as FunctionDefinition;
            }
            else
                ExceptionsHelper.Throw(new SyntaxError("Unknown syntax error"));
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        [Hidden]
        internal Function(Context context, FunctionDefinition creator)
        {
            attributes = JSValueAttributesInternal.ReadOnly | JSValueAttributesInternal.DoNotDelete | JSValueAttributesInternal.DoNotEnumerate | JSValueAttributesInternal.SystemObject;
            this.parentContext = context;
            this.creator = creator;
            valueType = JSValueType.Function;
            this.oValue = this;
        }

        protected internal virtual JSValue InternalInvoke(JSValue self, Expression[] arguments, Context initiator, bool withSpread)
        {
            if (!withSpread && this.GetType() == typeof(Function))
            {
                var body = creator.body;
                var result = notExists;
                notExists.valueType = JSValueType.NotExists;
                for (; ; )
                {
                    if (body != null)
                    {
                        if (body.lines.Length == 1)
                        {
                            var ret = body.lines[0] as ReturnStatement;
                            if (ret != null)
                            {
                                if (ret.Body != null)
                                {
                                    if (ret.Body.IsContextIndependent)
                                        result = ret.Body.Evaluate(null);
                                    else
                                        break;
                                }
                            }
                            else
                                break;
                        }
                        else if (body.lines.Length != 0)
                            break;
                    }
                    correctThisBind(self, body.strict); // нужно на случай вызова по new
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        if (!arguments[i].Evaluate(initiator).IsDefined)
                        {
                            if (creator.parameters.Length > i && creator.parameters[i].initializer != null)
                                creator.parameters[i].initializer.Evaluate(parentContext);
                        }
                    }
                    return result;
                }

                // быстро выполнить не получилось. 
                // Попробуем чуточку медленее
                if (creator != null
                    && !creator.statistic.ContainsArguments
                    && !creator.statistic.ContainsRestParameters
                    && !creator.statistic.ContainsEval
                    && !creator.statistic.ContainsWith
                    && creator.parameters.Length == arguments.Length // из-за необходимости иметь возможность построить аргументы, если они потребуются
                    && arguments.Length < 9)
                {
                    return fastInvoke(self, arguments, initiator);
                }
            }

            // Совсем медленно. Плохая функция попалась
            Arguments argumentsObject = new Core.Arguments(initiator);
            IList<JSValue> spreadSource = null;

            int targetIndex = 0;
            int sourceIndex = 0;
            int spreadIndex = 0;

            while (sourceIndex < arguments.Length)
            {
                if (spreadSource != null)
                {
                    argumentsObject[targetIndex] = spreadSource[spreadIndex];
                    spreadIndex++;
                    if (spreadIndex == spreadSource.Count)
                    {
                        spreadSource = null;
                        sourceIndex++;
                    }
                }
                else
                {
                    var value = CallOperator.PrepareArg(initiator, arguments[sourceIndex]);
                    if (value.valueType == JSValueType.SpreadOperatorResult)
                    {
                        spreadIndex = 0;
                        spreadSource = value.oValue as IList<JSValue>;
                        continue;
                    }
                    else
                    {
                        sourceIndex++;
                        argumentsObject[targetIndex] = value;
                    }
                }
                targetIndex++;
            }

            argumentsObject.length = targetIndex;

            initiator.objectSource = null;

            return Invoke(self, argumentsObject);
        }

        private JSValue fastInvoke(JSValue targetObject, Expression[] arguments, Context initiator)
        {
#if DEBUG && !PORTABLE
            if (creator.trace)
                System.Console.WriteLine("DEBUG: Run \"" + creator.Reference.Name + "\"");
#endif
            var body = creator.body;
            targetObject = correctThisBind(targetObject, body.strict);
            if (creator.recursionDepth > creator.parametersStored) // рекурсивный вызов.
            {
                storeParameters();
                creator.parametersStored++;
            }

            JSValue res = null;
            Arguments args = null;
            bool tailCall = true;
            for (; ; )
            {
                var internalContext = new Context(parentContext, false, this);
                if (creator.type == FunctionType.Arrow)
                    internalContext.thisBind = parentContext.thisBind;
                else
                    internalContext.thisBind = targetObject;
                if (tailCall)
                    initParametersFast(arguments, initiator, internalContext);
                else
                    initParameters(args, internalContext);

                // Эта строка обязательно должна находиться после инициализации параметров
                creator.recursionDepth++;

                if (this.creator.reference.descriptor != null && creator.reference.descriptor.cacheRes == null)
                {
                    creator.reference.descriptor.cacheContext = internalContext.parent;
                    creator.reference.descriptor.cacheRes = this;
                }
                internalContext.strict |= body.strict;
                internalContext.variables = body.variables;
                internalContext.Activate();
                try
                {
                    res = evaluate(internalContext);
                    if (internalContext.abortType == AbortType.TailRecursion)
                    {
                        tailCall = false;
                        args = internalContext.abortInfo as Arguments;
                    }
                    else
                        tailCall = true;
                }
                finally
                {
#if DEBUG && !PORTABLE
                    if (creator.trace)
                        System.Console.WriteLine("DEBUG: Exit \"" + creator.Reference.Name + "\"");
#endif
                    creator.recursionDepth--;
                    if (creator.parametersStored > creator.recursionDepth)
                        creator.parametersStored--;
                    exit(internalContext);
                }
                if (tailCall)
                    break;
                targetObject = correctThisBind(internalContext.objectSource, body.strict);
            }
            return res;
        }

        [Hidden]
        public virtual JSValue Invoke(JSValue targetObject, Arguments args)
        {
#if DEBUG && !PORTABLE
            if (creator.trace)
                System.Console.WriteLine("DEBUG: Run \"" + creator.Reference.Name + "\"");
#endif
            JSValue res = null;
            var body = creator.body;
            targetObject = correctThisBind(targetObject, body.strict);
            if (body.lines.Length == 0)
            {
                notExists.valueType = JSValueType.NotExists;
                return notExists;
            }
            var ceocw = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
            if (creator.recursionDepth > creator.parametersStored) // рекурсивный вызов.
            {
                if (!ceocw)
                    storeParameters();
                creator.parametersStored = creator.recursionDepth;
            }
            if (args == null)
            {
                var cc = Context.CurrentContext;
                args = new Arguments(cc);
            }
            for (; ; ) // tail recursion catcher
            {
                creator.recursionDepth++;
                var internalContext = new Context(parentContext, ceocw, this);
                internalContext.Activate();
                try
                {
                    if (this.creator.reference.descriptor != null && creator.reference.descriptor.cacheRes == null)
                    {
                        creator.reference.descriptor.cacheContext = internalContext.parent;
                        creator.reference.descriptor.cacheRes = this;
                    }
                    internalContext.thisBind = targetObject;
                    internalContext.strict |= body.strict;
                    internalContext.variables = body.variables;
                    if (creator.type == FunctionType.Arrow)
                    {
                        internalContext.arguments = internalContext.parent.arguments;
                        internalContext.thisBind = internalContext.parent.thisBind;
                    }
                    else
                    {
                        internalContext.arguments = args;
                        if (ceocw)
                            internalContext.fields["arguments"] = internalContext.arguments;
                        if (body.strict)
                        {
                            args.attributes |= JSValueAttributesInternal.ReadOnly;
                            args.callee = propertiesDummySM;
                            args.caller = propertiesDummySM;
                        }
                        else
                        {
                            args.callee = this;
                        }
                    }
                    initParameters(args, internalContext);
                    res = evaluate(internalContext);
                }
                finally
                {
#if DEBUG && !PORTABLE
                    if (creator.trace)
                        System.Console.WriteLine("DEBUG: Exit \"" + creator.Reference.Name + "\"");
#endif
                    creator.recursionDepth--;
                    if (creator.parametersStored > creator.recursionDepth)
                        creator.parametersStored = creator.recursionDepth;
                    exit(internalContext);
                }
                if (res != null) // tail recursion
                    break;
                args = internalContext.abortInfo as Arguments;
                targetObject = correctThisBind(internalContext.objectSource, body.strict);
            }
            return res;
        }

        private JSValue evaluate(Context internalContext)
        {
            initVariables(creator.body, internalContext);
            creator.body.Evaluate(internalContext);
            if (internalContext.abortType == AbortType.TailRecursion)
                return null;
            var ai = internalContext.abortInfo;
            if (ai == null || ai.valueType < JSValueType.Undefined)
            {
                notExists.valueType = JSValueType.NotExists;
                return notExists;
            }
            else if (ai.valueType == JSValueType.Undefined)
                return undefined;
            else
            {
                ai = ai.CloneImpl(false);
                ai.attributes |= JSValueAttributesInternal.Cloned;
                return ai;
            }
        }

        private void exit(Context internalContext)
        {
            if (internalContext.abortType != AbortType.TailRecursion && creator.recursionDepth == 0)
            {
                var i = creator.body.localVariables.Length;
                for (; i-- > 0; )
                {
                    creator.body.localVariables[i].cacheContext = null;
                    creator.body.localVariables[i].cacheRes = null;
                }
                for (i = creator.parameters.Length; i-- > 0; )
                {
                    creator.parameters[i].cacheContext = null;
                    creator.parameters[i].cacheRes = null;
                }
            }
            internalContext.abortType = AbortType.Return;
            internalContext.Deactivate();
        }

        private void initParametersFast(Expression[] arguments, Core.Context initiator, Context internalContext)
        {
            JSValue a0 = null,
                    a1 = null,
                    a2 = null,
                    a3 = null,
                    a4 = null,
                    a5 = null,
                    a6 = null,
                    a7 = null; // Вместо кучи, выделяем память на стеке

            var argumentsCount = arguments.Length;
            if (creator.parameters.Length != argumentsCount)
                throw new ArgumentException("Invalid arguments count");
            if (argumentsCount > 8)
                throw new ArgumentException("To many arguments");
            if (argumentsCount == 0)
                return;

            /*
             * Да, от этого кода можно вздрогнуть, но по ряду причин лучше сделать не получится.
             * Такая она цена оптимизации
             */

            /*
             * Эти два блока нельзя смешивать. Текущие значения параметров могут быть использованы для расчёта новых. 
             * Поэтому заменять значения можно только после полного расчёта новых значений
             */

            a0 = arguments[0].Evaluate(initiator).CloneImpl(false);
            if (argumentsCount > 1)
            {
                a1 = arguments[1].Evaluate(initiator).CloneImpl(false);
                if (argumentsCount > 2)
                {
                    a2 = arguments[2].Evaluate(initiator).CloneImpl(false);
                    if (argumentsCount > 3)
                    {
                        a3 = arguments[3].Evaluate(initiator).CloneImpl(false);
                        if (argumentsCount > 4)
                        {
                            a4 = arguments[4].Evaluate(initiator).CloneImpl(false);
                            if (argumentsCount > 5)
                            {
                                a5 = arguments[5].Evaluate(initiator).CloneImpl(false);
                                if (argumentsCount > 6)
                                {
                                    a6 = arguments[6].Evaluate(initiator).CloneImpl(false);
                                    if (argumentsCount > 7)
                                    {
                                        a7 = arguments[7].Evaluate(initiator).CloneImpl(false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            setParamValue(0, a0, internalContext);
            if (argumentsCount > 1)
            {
                setParamValue(1, a1, internalContext);
                if (argumentsCount > 2)
                {
                    setParamValue(2, a2, internalContext);
                    if (argumentsCount > 3)
                    {
                        setParamValue(3, a3, internalContext);
                        if (argumentsCount > 4)
                        {
                            setParamValue(4, a4, internalContext);
                            if (argumentsCount > 5)
                            {
                                setParamValue(5, a5, internalContext);
                                if (argumentsCount > 6)
                                {
                                    setParamValue(6, a6, internalContext);
                                    if (argumentsCount > 7)
                                    {
                                        setParamValue(7, a7, internalContext);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void setParamValue(int index, JSValue value, Context context)
        {
            if (!value.IsDefined && creator.parameters.Length > index && creator.parameters[index].initializer != null)
                value.Assign(creator.parameters[index].initializer.Evaluate(context));
            value.attributes |= JSValueAttributesInternal.Argument;
            creator.parameters[index].cacheRes = value;
            creator.parameters[index].cacheContext = context;
            if (creator.parameters[index].captured)
            {
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                context.fields[creator.parameters[index].name] = value;
            }
        }

        internal void BuildArgumentsObject()
        {
            var context = Context.GetRunnedContextFor(this);
            if (context != null && context.arguments == null)
            {
                var args = new Arguments()
                {
                    caller = context.oldContext != null ? context.oldContext.owner : null,
                    callee = this,
                    length = creator.parameters.Length
                };
                for (var i = 0; i < creator.parameters.Length; i++)
                {
                    if (creator.body.strict)
                        args[i] = creator.parameters[i].cacheRes.CloneImpl(false);
                    else
                        args[i] = creator.parameters[i].cacheRes;
                }
                context.arguments = args;
            }
        }

        private void initParameters(Arguments args, Context internalContext)
        {
            var cea = creator.statistic.ContainsEval || creator.statistic.ContainsArguments;
            var cew = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
            int min = System.Math.Min(args.length, creator.parameters.Length - (creator.statistic.ContainsRestParameters ? 1 : 0));

            Array restArray = null;
            if (creator.statistic.ContainsRestParameters)
            {
                restArray = new Array();
            }

            for (var i = 0; i < min; i++)
            {
                JSValue t = args[i];
                var prm = creator.parameters[i];
                if (!t.IsDefined && prm.initializer != null)
                    t = prm.initializer.Evaluate(internalContext);
                if (creator.body.strict)
                {
                    if (prm.assignations != null)
                    {
                        args[i] = t = t.CloneImpl(false);
                        t.attributes |= JSValueAttributesInternal.Cloned;
                    }
                    if (cea)
                    {
                        if ((t.attributes & JSValueAttributesInternal.Cloned) == 0)
                            args[i] = t.CloneImpl(false);
                        t = t.CloneImpl(false);
                    }
                }
                else
                {
                    if (prm.assignations != null
                        || cea
                        || cew
                        || (t.attributes & JSValueAttributesInternal.Temporary) != 0)
                    {
                        if ((t.attributes & JSValueAttributesInternal.Cloned) == 0)
                            args[i] = t = t.CloneImpl(false);
                        t.attributes |= JSValueAttributesInternal.Argument;
                    }
                }
                t.attributes &= ~JSValueAttributesInternal.Cloned;
                if (prm.captured || cew)
                    (internalContext.fields ?? (internalContext.fields = getFieldsContainer()))[prm.Name] = t;
                prm.cacheContext = internalContext;
                prm.cacheRes = t;
                if (string.CompareOrdinal(prm.name, "arguments") == 0)
                    internalContext.arguments = t;
            }

            for (var i = min; i < args.length; i++)
            {
                JSValue t = args[i];
                if (cew || cea)
                    args[i] = t = t.CloneImpl(false);
                t.attributes |= JSValueAttributesInternal.Argument;

                if (restArray != null)
                {
                    restArray.data.Add(t);
                }
            }

            for (var i = min; i < creator.parameters.Length; i++)
            {
                var arg = creator.parameters[i];
                if (arg.initializer != null)
                {
                    if (cew || arg.assignations != null)
                    {
                        arg.cacheRes = arg.initializer.Evaluate(internalContext).CloneImpl(false);
                    }
                    else
                    {
                        arg.cacheRes = arg.initializer.Evaluate(internalContext);
                        if (!arg.cacheRes.IsDefined)
                            arg.cacheRes = undefined;
                    }
                }
                else
                {
                    if (cew || arg.assignations != null)
                    {
                        if (i == min && restArray != null)
                            arg.cacheRes = restArray.CloneImpl(false);
                        else
                            arg.cacheRes = new JSValue() { valueType = JSValueType.Undefined };
                        arg.cacheRes.attributes = JSValueAttributesInternal.Argument;
                    }
                    else
                    {
                        if (i == min && restArray != null)
                            arg.cacheRes = restArray;
                        else
                            arg.cacheRes = JSValue.undefined;
                    }
                }
                arg.cacheContext = internalContext;
                if (arg.captured || cew)
                {
                    if (internalContext.fields == null)
                        internalContext.fields = getFieldsContainer();
                    internalContext.fields[arg.Name] = arg.cacheRes;
                }
                if (string.CompareOrdinal(arg.name, "arguments") == 0)
                    internalContext.arguments = arg.cacheRes;
            }
        }

        private void initVariables(CodeBlock body, Context internalContext)
        {
            if (body.localVariables != null)
            {
                var cew = creator.statistic.ContainsEval || creator.statistic.ContainsWith;
                for (var i = body.localVariables.Length; i-- > 0; )
                {
                    var v = body.localVariables[i];
                    bool isArg = string.CompareOrdinal(v.name, "arguments") == 0;
                    if (isArg && v.initializer == null)
                        continue;
                    JSValue f = new JSValue() { valueType = JSValueType.Undefined, attributes = JSValueAttributesInternal.DoNotDelete };
                    if (v.captured || cew)
                        (internalContext.fields ?? (internalContext.fields = getFieldsContainer()))[v.name] = f;
                    if (v.initializer != null)
                        f.Assign(v.initializer.Evaluate(internalContext));
                    if (v.isReadOnly)
                        f.attributes |= JSValueAttributesInternal.ReadOnly;
                    v.cacheRes = f;
                    v.cacheContext = internalContext;
                    if (isArg)
                        internalContext.arguments = f;
                }
            }
        }

        internal JSValue correctThisBind(JSValue thisBind, bool strict)
        {
            if (thisBind == null)
                return strict ? undefined : parentContext.Root.thisBind;
            else if (thisBind.oValue == typeof(NewOperator) as object)
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.OnlyWithoutNew)
                    ExceptionsHelper.Throw(new TypeError(string.Format(Strings.InvalidTryToCreateWithNew, name)));
                thisBind.__proto__ = prototype.valueType < JSValueType.Object ? GlobalPrototype : prototype.oValue as JSObject;
                thisBind.oValue = thisBind;
            }
            else if (parentContext != null)
            {
                if (RequireNewKeywordLevel == BaseLibrary.RequireNewKeywordLevel.OnlyWithNew)
                    ExceptionsHelper.Throw(new TypeError(string.Format(Strings.InvalidTryToCreateWithoutNew, name)));
                if (!strict) // Поправляем this
                {
                    if (thisBind.valueType > JSValueType.Undefined && thisBind.valueType < JSValueType.Object)
                        return thisBind.ToObject();
                    else if (thisBind.valueType <= JSValueType.Undefined || thisBind.oValue == null)
                        return parentContext.Root.thisBind;
                }
                else if (thisBind.valueType <= JSValueType.Undefined)
                    return undefined;
            }
            return thisBind;
        }

        private void storeParameters()
        {
            if (creator.parameters.Length != 0)
            {
                var context = creator.parameters[0].cacheContext;
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                for (var i = 0; i < creator.parameters.Length; i++)
                    context.fields[creator.parameters[i].Name] = creator.parameters[i].cacheRes;
            }
            if (creator.body.localVariables != null && creator.body.localVariables.Length > 0)
            {
                var context = creator.body.localVariables[0].cacheContext;
                if (context.fields == null)
                    context.fields = getFieldsContainer();
                for (var i = 0; i < creator.body.localVariables.Length; i++)
                    context.fields[creator.body.localVariables[i].Name] = creator.body.localVariables[i].cacheRes;
            }
        }

        [Hidden]
        public JSValue Invoke(Arguments args)
        {
            return Invoke(undefined, args);
        }

        [Hidden]
        internal protected override JSValue GetMember(JSValue nameObj, bool forWrite, MemberScope memberScope)
        {
            if (nameObj.valueType != JSValueType.Symbol)
            {
                string name = nameObj.ToString();
                if (creator.body.strict && (name == "caller" || name == "arguments"))
                    return propertiesDummySM;
                if ((attributes & JSValueAttributesInternal.ProxyPrototype) != 0 && name == "prototype")
                    return prototype;
            }
            return base.GetMember(nameObj, forWrite, memberScope);
        }

        [CLSCompliant(false)]
        [DoNotEnumerate]
        [ArgumentsLength(0)]
        public new virtual JSValue toString(Arguments args)
        {
            return ToString();
        }

        [Hidden]
        public override sealed string ToString()
        {
            return ToString(false);
        }

        [Hidden]
        public virtual string ToString(bool headerOnly)
        {
            StringBuilder res = new StringBuilder();
            switch (creator.type)
            {
                case FunctionType.Generator:
                    res.Append("function*");
                    break;
                case FunctionType.Get:
                    res.Append("get");
                    break;
                case FunctionType.Set:
                    res.Append("set");
                    break;
                default:
                    res.Append("function");
                    break;
            }
            res.Append(" ").Append(name).Append("(");
            if (creator != null && creator.parameters != null)
                for (int i = 0; i < creator.parameters.Length; )
                    res.Append(creator.parameters[i].Name).Append(++i < creator.parameters.Length ? "," : "");
            res.Append(")");
            if (!headerOnly)
                res.Append(' ').Append(creator != creatorDummy ? creator.body as object : "{ [native code] }");
            return res.ToString();
        }

        [Hidden]
        public override JSValue valueOf()
        {
            return base.valueOf();
        }

        [DoNotEnumerate]
        public JSValue call(Arguments args)
        {
            var newThis = args[0];
            var prmlen = args.length - 1;
            if (prmlen >= 0)
            {
                for (var i = 0; i <= prmlen; i++)
                    args[i] = args[i + 1];
                args[prmlen] = null;
                args.length = prmlen;
            }
            else
                args[0] = null;
            return Invoke(newThis, args);
        }

        [DoNotEnumerate]
        [ArgumentsLength(2)]
        [AllowNullArguments]
        public JSValue apply(Arguments args)
        {
            var nargs = args ?? new Arguments();
            var argsSource = nargs[1];
            var self = nargs[0];
            if (args != null)
                nargs.Reset();
            if (argsSource.IsDefined)
            {
                if (argsSource.valueType < JSValueType.Object)
                    ExceptionsHelper.Throw(new TypeError("Argument list has wrong type."));
                var len = argsSource["length"];
                if (len.valueType == JSValueType.Property)
                    len = (len.oValue as PropertyPair).get.Invoke(argsSource, null);
                nargs.length = Tools.JSObjectToInt32(len);
                if (nargs.length >= 50000)
                    ExceptionsHelper.Throw(new RangeError("Too many arguments."));
                for (var i = nargs.length; i-- > 0; )
                    nargs[i] = argsSource[Tools.Int32ToString(i)];
            }
            return Invoke(self, nargs);
        }

        [DoNotEnumerate]
        public JSValue bind(Arguments args)
        {
            var newThis = args.Length > 0 ? args[0] : null;
            var strict = (creator.body != null && creator.body.strict) || Context.CurrentContext.strict;
            if ((newThis != null && newThis.valueType > JSValueType.Undefined) || strict)
                return new BindedFunction(this, args);
            return this;
        }

        [Hidden]
        public virtual object MakeDelegate(Type delegateType)
        {
#if PORTABLE
            throw new NotSupportedException("Do not supported in portable version");
#else
            var del = delegateType.GetMethod("Invoke");
            var prms = del.GetParameters();
            if (prms.Length <= 16)
            {
                var invokes = typeof(_DelegateWraper).GetMember("Invoke");
                if (del.ReturnType != typeof(void))
                {
                    Type[] argtypes = new Type[prms.Length + 1];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i + 1] = prms[i].ParameterType;
                    argtypes[0] = del.ReturnType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)invokes[prms.Length]).MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
                else
                {
                    Type[] argtypes = new Type[prms.Length];
                    for (int i = 0; i < prms.Length; i++)
                        argtypes[i] = prms[i].ParameterType;
                    var instance = new _DelegateWraper(this);
                    var method = ((System.Reflection.MethodInfo)invokes[17 + prms.Length]);
                    if (prms.Length > 0)
                        method = method.MakeGenericMethod(argtypes);
                    return Delegate.CreateDelegate(delegateType, instance, method);
                }
            }
            else
                throw new ArgumentException("Parameters count must be less or equal 16.");
#endif
        }
    }
}
