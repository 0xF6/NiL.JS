﻿using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace NiL.JSTest
{
    class Program
    {
        private static void benchmark()
        {
            const int iterations = 100000000;
            Console.WriteLine("iterations count: " + iterations);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Script s = new Script(@"
var a = 1; for(var i = 0; i < " + iterations + @";i++){ a = a * i + 3 - 2 / 2; }
");
            sw.Stop();
            var init = sw.Elapsed;
            sw.Restart();
            s.Invoke();
            sw.Stop();
            var l = sw.Elapsed;
            Console.WriteLine("script: " + (sw.Elapsed.Ticks / 10000).ToString());
            Console.WriteLine("initialization: " + (init.Ticks / 10000).ToString());
            var a = 1.0;
            sw.Restart();
            for (var i = 0; i < iterations; i++)
                a = a * i + 3 - 2 / 2;
            sw.Stop();
            Console.WriteLine(a == Tools.JSObjectToDouble(s.Context.GetVariable("a")) ? "valid" : "invalid");
            Console.WriteLine("native: " + (sw.Elapsed.Ticks / 10000).ToString());
            Console.WriteLine("rate: " + ((double)l.Ticks / (double)sw.Elapsed.Ticks).ToString());
        }

        private static void runFile(string filename)
        {
            Console.WriteLine("Processing file: " + filename);
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var s = new Script(sr.ReadToEnd());
            sw.Stop();
            Console.WriteLine("Compile time: " + sw.Elapsed);
            s.Context.DefineVariable("$ERROR").Assign(new ExternalFunction((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetMember("0").Value);
                return null;
            }));
            s.Context.DefineVariable("ERROR").Assign(new ExternalFunction((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetMember("0").Value);
                return null;
            }));
            s.Context.DefineVariable("$PRINT").Assign(new ExternalFunction((t, x) =>
            {
                Console.WriteLine("PRINT: " + x.GetMember("0").Value);
                return null;
            }));
            s.Context.DefineVariable("$FAIL").Assign(new ExternalFunction((t, x) =>
            {
                Console.WriteLine("FAIL: " + x.GetMember("0").Value);
                return null;
            }));
            Console.WriteLine("-------------------------------------");
            s.Invoke();
            sr.Dispose();
            f.Dispose();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
        }

        private static void sputnicTests(string folderPath = "tests\\")
        {
            Action<string> _ = Console.WriteLine;
            var sw = new Stopwatch();
            int passed = 0;
            int failed = 0;
            string code;
            bool negative = false;
            _("Sputnik testing begin...");
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");
            sw.Start();
            for (int i = 0; i < fls.Length; i++)
            {
                bool pass = true;
                try
                {
                    Console.Write("Processing file \"" + fls[i] + "\" ");
                    Context.RefreshGlobalContext();
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    code = sr.ReadToEnd() + "\nfunction runTestCase(a){ if (!a()) ERROR('Test failed') };function fnGlobalObject(){return this};";
                    negative = code.IndexOf("@negative") != -1;
                    if (negative)
                        pass = false;
                    var s = new Script(code);
                    s.Context.DefineVariable("$ERROR").Assign(new ExternalFunction((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetMember("0").Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.DefineVariable("ERROR").Assign(new ExternalFunction((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetMember("0").Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.DefineVariable("PRINT").Assign(new ExternalFunction((t, x) =>
                    {
                        Console.WriteLine("PRINT: " + x.GetMember("0").Value);
                        return null;
                    }));
                    s.Context.DefineVariable("$PRINT").Assign(new ExternalFunction((t, x) =>
                    {
                        Console.WriteLine("PRINT: " + x.GetMember("0").Value);
                        return null;
                    }));
                    s.Context.DefineVariable("$FAIL").Assign(new ExternalFunction((t, x) =>
                    {
                        Console.WriteLine("FAIL: " + x.GetMember("0").Value);
                        pass = false;
                        return null;
                    }));
                    s.Invoke();
                    sr.Dispose();
                    f.Dispose();
                }
                catch (NotImplementedException e)
                {
                    pass = false;
                }
                catch (ArgumentException)
                {
                    pass = false;
                }
                catch (NullReferenceException)
                {
                    pass = false;
                }
                catch (Exception e)
                {
                    if (!negative)
                        pass = false;
                    else
                        pass = true;
                }
                if (pass)
                {
                    _("Passed");
                    passed++;
                }
                else
                {
                    _("Failed");
                    failed++;
                }
                Console.Title = "passed: " + passed + ". failed: " + failed;
                if (failed == 3) break;
            }
            sw.Stop();
            _("passed: " + passed);
            _("failed: " + failed);
            _("time: " + sw.Elapsed);
            _("Sputnik testing complite");
        }

        private sealed class DoubleStringConverter : NiL.JS.Core.Modules.ConvertValueAttribute
        {
            public override object From(object source)
            {
                return ((double)source).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            public override object To(object source)
            {
                return double.Parse(source as string, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private class TestClass
        {
            [DoubleStringConverter]
            public double Value
            {
                get { return 1.0; }

                set { }
            }

            void test(JSObject o)
            {

            }

            void test(object o)
            {

            }
        }

        private static void testEx()
        {
            var sw = new Stopwatch();
            var s = new Script(
@"
(1,Object.prototype.valueOf)();
");
            s.Context.AttachModule(typeof(TestClass));
            sw.Start();
            s.Invoke();
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        static void Main(string[] args)
        {
            typeof(System.Windows.Forms.Button).GetType();
            NiL.JS.Core.Context.GlobalContext.DefineVariable("platform").Assign("NiL.JS");
            NiL.JS.Core.Context.GlobalContext.DefineVariable("System").Assign(new NamespaceProvider("System"));
            NiL.JS.Core.Context.GlobalContext.DefineVariable("load").Assign(new ExternalFunction((context, eargs) =>
            {
                var f = new FileStream("Benchmarks\\" + eargs.GetMember("0").ToString(), FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(f);
                return Context.CurrentContext.Eval(sr.ReadToEnd());
            }));
            //benchmark();
            //runFile(@"ftest.js");
            //runFile(@"Benchmarks\run.js");
            //runFile(@"C:\Users\Дмитрий\Documents\Projects\NiL.JS\NiL.JSTest\tests\sputnik\ch08\8.5\8.5.1.js");
            sputnicTests();
            //testEx();
            //runFile(@"C:\Users\Дмитрий\Documents\Projects\NiL.JS\NiL.JSTest\tests\Conformance\08_Types\8.7_The_Reference_Type\S8.7_A3.js");
            //sputnicTests(@"tests\Conformance\15_Native_ECMA_Script_Objects\15.5_String_Objects");
            //sputnicTests(@"tests\Conformance\15_Native_ECMA_Script_Objects\15.2_Object_Objects");
            //sputnicTests(@"tests\Conformance\15_Native_ECMA_Script_Objects\15.4_Array_Objects");
            //sputnicTests(@"tests\Conformance\15_Native_ECMA_Script_Objects\15.7_Number_Objects");
            //sputnicTests(@"tests\Conformance\15_Native_ECMA_Script_Objects\15.12_The_JSON_Object");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC.GetTotalMemory: " + GC.GetTotalMemory(true));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(0));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(1));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(2));
            Console.WriteLine("GC.MaxGeneration: " + GC.MaxGeneration);
            if (System.Windows.Forms.Application.OpenForms.Count != 0)
            {
                while (System.Windows.Forms.Application.OpenForms.Count != 0)
                {
                    System.Threading.Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            else Console.ReadKey();
        }
    }
}
