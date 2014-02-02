﻿using NiL.JS;
using NiL.JS.Core;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NiL.JSTest
{
    class Program
    {
        private static void benchmark()
        {
            const int iterations = 100000000;
            Console.WriteLine("iterations count: " + iterations);

            long init = DateTime.Now.Ticks;
            Script s = new Script(@"
var a = 1; for(var i = 0; i < " + iterations + @";i++){ a = a * i + 3 - 2 / 2; }
");
            init = DateTime.Now.Ticks - init;
            long start = DateTime.Now.Ticks;
            s.Invoke();
            long l = (DateTime.Now.Ticks - start);
            Console.WriteLine("script: " + (l / 10000).ToString());
            Console.WriteLine("initialization: " + (init / 10000).ToString());
            var a = 1.0;
            long nativeStart = DateTime.Now.Ticks;
            for (var i = 0; i < iterations; i++)
                a = a * i + 3 - 2 / 2;
            long nativeL = (DateTime.Now.Ticks - nativeStart);
            Console.WriteLine(a == Tools.JSObjectToDouble(s.Context.GetField("a")) ? "valid" : "invalid");
            Console.WriteLine("native: " + (nativeL / 10000).ToString());
            Console.WriteLine("rate: " + ((double)l / (double)nativeL).ToString());
        }

        private static void runFile(string filename)
        {
            Console.WriteLine("Processing file: " + filename);
            Console.WriteLine("-------------------------------------");
            var f = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(f);
            var s = new Script(sr.ReadToEnd());
            s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Context.GetField("ERROR").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Context.GetField("$PRINT").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("PRINT: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Context.GetField("$FAIL").Assign(new CallableField((t, x) =>
            {
                Console.WriteLine("FAIL: " + x.GetField("0", true, false).Value);
                return null;
            }));
            s.Invoke();
            sr.Dispose();
            f.Dispose();
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Complite.");
        }

        private static void sputnicTests(string folderPath = "tests\\")
        {
            Action<string> _ = Console.WriteLine;

            int passed = 0;
            int failed = 0;
            string code;
            bool negative = false;
            _("Sputnik testing begin...");
            _("Directory: \"" + Directory.GetParent(folderPath) + "\"");

            _("Scaning directory...");
            var fls = Directory.EnumerateFiles(folderPath, "*.js", SearchOption.AllDirectories).ToArray();
            _("Founded " + fls.Length + " js-files");

            for (int i = 0; i < fls.Length; i++)
            {
                bool pass = true;
                try
                {
                    Console.Write("Processing file \"" + fls[i] + "\" ");
                    Context.RefreshGlobalContext();
                    var f = new FileStream(fls[i], FileMode.Open, FileAccess.Read);
                    var sr = new StreamReader(f);
                    code = "function runTestCase(a){a()};\n" + sr.ReadToEnd();
                    negative = code.IndexOf("@negative") != -1;
                    if (negative)
                        pass = false;
                    var s = new Script(code);
                    s.Context.GetField("$ERROR").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.GetField("ERROR").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("ERROR: " + x.GetField("0", true, false).Value);
                        pass = false;
                        return null;
                    }));
                    s.Context.GetField("$PRINT").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("PRINT: " + x.GetField("0", true, false).Value);
                        return null;
                    }));
                    s.Context.GetField("$FAIL").Assign(new CallableField((t, x) =>
                    {
                        Console.WriteLine("FAIL: " + x.GetField("0", true, false).Value);
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
            }

            _("passed: " + passed);
            _("failed: " + failed);

            _("Sputnik testing complite");
            Console.ReadLine();
        }

        private class TestClass
        {
            public float dfield = 1;
            public static float sfield = 2.468f;

            public static void smethod()
            {

            }

            public void dmethod(double a)
            {
                dfield = (float)a;
            }

            public void dmethod(float a)
            {
                dfield = a;
            }
        }

        private static void testEx()
        {
            Context.GlobalContext.AttachModule(typeof(TestClass));
            var s = new Script(@"
var d = new Date('January 11, 1980 06:30:00');
gmt = d.toGMTString();
utc = d.toUTCString();
console.log(gmt);
console.log(utc);
");
            s.Invoke();
        }

        static void Main(string[] args)
        {
            typeof(System.Windows.Forms.Button).GetType();
            NiL.JS.Core.Context.GlobalContext.GetOwnField("platform").Assign("NiL.JS");
            NiL.JS.Core.Context.GlobalContext.GetOwnField("System").Assign(new SubspaceProvider("System"));
            NiL.JS.Core.Context.GlobalContext.GetOwnField("load").Assign(new CallableField((context, eargs) =>
            {
                var f = new FileStream("Benchmarks\\" + eargs.GetField("0", true, false).ToString(), FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(f);
                eargs.GetField("0", true, false).Assign(sr.ReadToEnd());
                return NiL.JS.Core.Context.Eval(context, eargs);
            }));
            //benchmark();
            //runFile(@"ftest.js");
            runFile(@"Benchmarks\run.js");
            //sputnicTests();
            //sputnicTests(@"tests\Conformance\07_Lexical_Conventions\7.5_Tokens\7.5.3_Future_Reserved_Words");
            //testEx();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(0));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(1));
            Console.WriteLine("GC.CollectionCount: " + GC.CollectionCount(2));
            Console.WriteLine("GC.MaxGeneration: " + GC.MaxGeneration);
            Console.WriteLine("GC.GetTotalMemory: " + GC.GetTotalMemory(false));
        }
    }
}
