using NLua;
using System;
using System.Runtime.InteropServices;

namespace ConsoleTest
{

    public static class CCallback
    {
        public const string LIBNAME = "CTest";

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CreateLuaState")]
        internal static extern void CreateLuaState();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "YieldLua")]
        internal static extern IntPtr YieldLua();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitLuaState")]
        internal static extern void InitLuaState();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CallCS")]
        internal static extern void LetCCallCS();

        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "CCallCSStressTest")]
        internal static extern void CCallCSStressTest();


        [DllImport(LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "empty_func")]
        internal static extern void CallCEmptyDirectly(IntPtr lua_state);

    }

    public class Program
    {
        public static Lua s_lua;
        public static int test_int_in_cs = -1;

        public static void FillIntToEditor(string name, int val)
        {
            // fill the property to editor here
            Console.WriteLine("Editor set property with name {0} to value {1}", name, val);
            test_int_in_cs = val;
        }

        public static void PullIntFromEditor(string name)
        {
            // set property to lua here
            Console.WriteLine("Set lua world's property with name {0} to value {1}", name, test_int_in_cs);
            s_lua[name] = test_int_in_cs;
        }

        public static void CSharpCallback()
        {
            Console.WriteLine("I'm C#'s CSharpCallback function. I guess I'm called by C?");
        }

        public static void CSharpCallbackEmpty()
        {
        }


        public delegate void DumbFunc();
        public delegate void FillFunc<T>(string name, T val);
        public delegate void PullFunc<T>(string name);
        public class Dogoa
        {
            public int qq = 3;
            public void QQ()
            {
                Console.WriteLine("From QQ");
            }
        }

        public static void Shifta(string x) { Console.WriteLine("Shifta"); }
        delegate void Aha(string x);

        static void Main(string[] args)
        {
            CCallback.CreateLuaState();
            CCallback.InitLuaState();
            s_lua = new Lua(CCallback.YieldLua());
            s_lua["dogoa"] = new Dogoa();
            s_lua["callmeback"] = (Aha)Shifta;
            s_lua.DoString("callmeback('dogama')");

            /////////////////
            Console.WriteLine();
            Console.WriteLine("[Demo Set Property]");
            Console.WriteLine();
            /////////////////
            s_lua["FillIntToEditor"] = (FillFunc<int>)FillIntToEditor;
            s_lua["PullIntFromEditor"] = (PullFunc<int>)PullIntFromEditor;

            Console.WriteLine("before getting property from lua, test_int_in_cs is {0}", test_int_in_cs);
            s_lua.DoString("FillIntToEditor('test_int', test_int)");
            Console.WriteLine("after getting property from lua, test_int_in_cs is {0}", test_int_in_cs);
            Console.WriteLine();

            Console.WriteLine("set test_int_in_cs to 100");
            test_int_in_cs = 100;
            Console.WriteLine("before changing lua world value, lua's test_int is {0}", s_lua["test_int"]);
            s_lua.DoString("PullIntFromEditor('test_int')");
            Console.WriteLine("after changing lua world value, lua's test_int is {0}", s_lua["test_int"]);

            /////////////////
            Console.WriteLine();
            Console.WriteLine("[Demo C# Call C++ Method From Lua]");
            Console.WriteLine();
            /////////////////

            Console.WriteLine("I'm in C#, ready to call a function in C from Lua...");
            s_lua.DoString("print('This is lua, trying to call CallBackInC...')");
            s_lua.DoString("CallBackInC()");

            Console.WriteLine();
            Console.WriteLine("I'm in C#, I can also avoid DoString and call CallBackInC this way...");
            s_lua.GetFunction("CallBackInC").Call();

            /////////////////
            Console.WriteLine();
            Console.WriteLine("[Demo C++ Call C# Method From Lua]");
            Console.WriteLine();
            /////////////////
            s_lua["CSharpCallback"] = (DumbFunc)CSharpCallback;
            Console.WriteLine("This is C#, Try to let C call C#...");
            CCallback.LetCCallCS();

            /////////////////
            Console.WriteLine();
            Console.WriteLine("[Stress test]");
            Console.WriteLine();
            /////////////////
            var stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();
            s_lua["CSharpCallbackEmpty"] = (DumbFunc)CSharpCallbackEmpty;
            CCallback.CCallCSStressTest();
            stopwatch.Stop();
            Console.WriteLine("Call from C to C# 10000 times takes {0} ms", stopwatch.Elapsed.Milliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < 10000; ++i)
            {
                var func = s_lua.GetFunction("CCallbackEmpty");
                func.Call();
            }
            stopwatch.Stop();
            Console.WriteLine("Call from C# to C 10000 times takes {0} ms", stopwatch.Elapsed.Milliseconds);

            var ls = CCallback.YieldLua();
            stopwatch.Reset();
            stopwatch.Start();
            for (int i = 0; i < 100000; ++i)
                CCallback.CallCEmptyDirectly(ls);
            stopwatch.Stop();
            Console.WriteLine("Call from C# to C 10000 times DIRECTLY takes {0} ms", stopwatch.Elapsed.Milliseconds);

            /////////////////
            // Lua call stress test
            /////////////////
            stopwatch.Reset();
            stopwatch.Start();
            s_lua.DoString("for i = 1, 10000 do CSharpCallbackEmpty() end");
            stopwatch.Stop();
            Console.WriteLine("Call from Lua to C# 10000 times takes {0} ms", stopwatch.Elapsed.Milliseconds);

            stopwatch.Reset();
            stopwatch.Start();
            s_lua.DoString("for i = 1, 10000 do CCallbackEmpty() end");
            stopwatch.Stop();
            Console.WriteLine("Call from Lua to C 10000 times takes {0} ms", stopwatch.Elapsed.Milliseconds);
        }
    }
}
