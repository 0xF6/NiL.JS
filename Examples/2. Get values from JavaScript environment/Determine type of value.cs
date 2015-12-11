﻿using System;
using NiL.JS.Core;
using NiL.JS.Extensions;

namespace Examples._2_Get_values_from_JavaScript_environment
{
    public sealed class Determine_type_of_value : ExamplesFramework.Example
    {
        public override void Run()
        {
            var context = new Context();

            context.DefineVariable("x").Assign(123);
            context.Eval("var result = x * 2");

            var result = context.GetVariable("result");

            Console.WriteLine("Result is integer: " + result.Is<int>()); // using NiL.JS.Extensions; Console: is result integer: True
            Console.WriteLine("Type of result: " + result.ValueType); // Console: Type of result: Integer
        }
    }
}
