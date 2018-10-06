using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Core.Interop;

namespace NiL.JS
{
    [Flags]
    public enum Options
    {
        None = 0,
        SuppressUselessExpressionsElimination = 1,
        SuppressUselessStatementsElimination = 2,
        SuppressConstantPropogation = 4,
    }

    /// <summary>
    /// Represents and manages JavaScript module
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class Module
    {
        private static readonly char[] _pathSplitChars = new[] { '\\', '/' };
        private static readonly StringMap<Module> _modulesCache = new StringMap<Module>();
        private static List<ResolveModuleHandler> _resolveModuleHandlers = new List<ResolveModuleHandler> { defaultModuleResolver };

        /// <summary>
        /// Occurs when module not found in cache
        /// </summary>
        public static event ResolveModuleHandler ResolveModule
        {
            add
            {
                if (value != null)
                    lock (_resolveModuleHandlers)
                        _resolveModuleHandlers.Add(value);
            }
            remove
            {
                lock (_resolveModuleHandlers)
                    _resolveModuleHandlers.Remove(value);
            }
        }

        public ExportTable Exports { get; } = new ExportTable();

        private CodeBlock _root;
        /// <summary>
        /// Root node of AST
        /// </summary>
        public CodeBlock Root { get { return _root; } }

        /// <summary>
        /// JavaScript code, used for initialization
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Root context of module
        /// </summary>
        public Context Context { get; private set; }

        /// <summary>
        /// Path to file with script
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        public Module(string code)
            : this(code, null, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code.
        /// </summary>
        /// <param name="path">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        public Module(string path, string code)
            : this(path, code, null, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code and callback for output compiler messages.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages</param>
        public Module(string code, CompilerMessageCallback messageCallback)
            : this(code, messageCallback, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code and callback for output compiler messages.
        /// </summary>
        /// <param name="path">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages</param>
        public Module(string path, string code, CompilerMessageCallback messageCallback)
            : this(path, code, messageCallback, Options.None)
        { }

        /// <summary>
        /// Initializes a new Module with specified code, callback for output compiler messages and compiler options.
        /// </summary>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        public Module(string code, CompilerMessageCallback messageCallback, Options options)
            : this(null, code, messageCallback, options)
        { }

        /// <summary>
        /// Initializes a new Module with specified code, callback for output compiler messages and compiler options.
        /// </summary>
        /// <param name="path">Path to file with script. Used for resolving paths to other modules for importing via import directive. Can be null or empty</param>
        /// <param name="code">JavaScript code.</param>
        /// <param name="messageCallback">Callback used to output compiler messages or null</param>
        /// <param name="options">Compiler options</param>
        public Module(string path, string code, CompilerMessageCallback messageCallback, Options options)
        {
            if (code == null)
                throw new ArgumentNullException();

            Code = code;
            Context = new Context(Context.CurrentGlobalContext, true, null);
            Context._module = this;
            if (!string.IsNullOrWhiteSpace(path))
            {
                lock (_modulesCache)
                {
                    if (!_modulesCache.ContainsKey(path))
                        _modulesCache[path] = this;
                }

                FilePath = path;
            }

            if (code == "")
                return;

            var internalCallback = messageCallback != null ? 
                (level, position, length, message) => messageCallback(level, CodeCoordinates.FromTextPosition(code, position, length), message)
                : null as InternalCompilerMessageCallback;

            int i = 0;
            _root = (CodeBlock)CodeBlock.Parse(new ParseInfo(Parser.RemoveComments(code, 0), Code, internalCallback), ref i);

            var stat = new FunctionInfo();
            Parser.Build(ref _root, 0, new Dictionary<string, VariableDescriptor>(), CodeContext.None, internalCallback, stat, options);
            var body = _root as CodeBlock;
            body._suppressScopeIsolation = SuppressScopeIsolationMode.Suppress;
            Context._thisBind = new GlobalObject(Context);
            Context._strict = body._strict;

            var tv = stat.WithLexicalEnvironment ? null : new Dictionary<string, VariableDescriptor>();
            body.RebuildScope(stat, tv, body._variables.Length == 0 || !stat.WithLexicalEnvironment ? 1 : 0);
            var bd = body as CodeNode;
            body.Optimize(ref bd, null, internalCallback, options, stat);
            if (tv != null)
                body._variables = new List<VariableDescriptor>(tv.Values).ToArray();

            if (stat.NeedDecompose)
                body.Decompose(ref bd);
        }

        public Module()
            : this("")
        {
        }

        /// <summary>
        /// Run the script
        /// </summary>
        public void Run()
        {
            if (Code == "")
                return;
            
            try
            {
                Context.Activate();
                _root.Evaluate(Context);
            }
            finally
            {
                Context.Deactivate();
            }
        }

        /// <summary>
        /// Run the script with time limit
        /// </summary>
        /// <param name="timeLimitInMilliseconds">Time limit</param>
        public void Run(int timeLimitInMilliseconds)
        {
            var start = Environment.TickCount;
            var oldDebugValue = Context.Debugging;
            Context.Debugging = true;
            DebuggerCallback callback = (context, e) =>
            {
                if (Environment.TickCount - start >= timeLimitInMilliseconds)
                    throw new TimeoutException();
            };
            Context.DebuggerCallback += callback;

            try
            {
                Run();
            }
            finally
            {
                Context.Debugging = oldDebugValue;
                Context.DebuggerCallback -= callback;
            }
        }

        internal Module Import(string path)
        {
            path = processPath(path);

            var e = new ResolveModuleEventArgs(path);

            for (var i = 0; i < _resolveModuleHandlers.Count && e.Module == null; i++)
                _resolveModuleHandlers[i](this, e);

            if (e.Module == null)
                throw new InvalidOperationException("Unable to load module \"" + path + "\"");

            if (e.AddToCache && !_modulesCache.ContainsKey(e.ModulePath))
                _modulesCache[e.ModulePath] = e.Module;

            if (e.Module.FilePath == null)
                e.Module.FilePath = path;

            return e.Module;
        }

        private string processPath(string path)
        {
            var thisName = this.FilePath.Split(_pathSplitChars);
            var requestedName = path.Split(_pathSplitChars);
            var pathTokens = new LinkedList<string>(thisName);

            if (requestedName.Length > 0 && requestedName[0] == "")
                pathTokens.Clear();
            else
                pathTokens.RemoveLast();

            for (var i = 0; i < requestedName.Length; i++)
                pathTokens.AddLast(requestedName[i]);

            for (var node = pathTokens.First; node != null;)
            {
                if (node.Value == "." || node.Value == "")
                {
                    node = node.Next;
                    pathTokens.Remove(node.Previous);
                }
                else if (node.Value == ".." && node.Previous != null)
                {
                    node = node.Next;
                    pathTokens.Remove(node.Previous);
                    pathTokens.Remove(node.Previous);
                }
                else
                    node = node.Next;
            }

            if (pathTokens.Last.Value.IndexOf('.') == -1)
                pathTokens.Last.Value = pathTokens.Last.Value + ".js";

            pathTokens.AddFirst("");
            path = string.Join("/", pathTokens);
            return path;
        }

        private static void defaultModuleResolver(Module sender, ResolveModuleEventArgs e)
        {
            Module result;
            _modulesCache.TryGetValue(e.ModulePath, out result);
            e.Module = result;
        }

        public static void ClearModuleCache()
        {
            lock (_modulesCache)
            {
                _modulesCache.Clear();
            }
        }

        public static bool RemoveFromModuleCache(string path)
        {
            lock (_modulesCache)
            {
                return _modulesCache.Remove(path);
            }
        }

#if !PORTABLE
        /// <summary>
        /// Returns module, which provides access to clr-namespace
        /// </summary>
        /// <param name="namespace">Namespace</param>
        /// <returns></returns>
        public static Module ClrNamespace(string @namespace)
        {
            var result = new Module();

            foreach (var type in NamespaceProvider.GetTypesByPrefix(@namespace))
            {
                try
                {
                    if (type.Namespace == @namespace)
                    {
                        result.Exports[type.Name] = Context.CurrentGlobalContext.GetConstructor(type);
                    }
                    else if (type.Namespace.StartsWith(@namespace) && type.Namespace[@namespace.Length] == '.')
                    {
                        var nextSegment = type.Namespace.Substring(@namespace.Length).Split('.')[1];
                        result.Exports[nextSegment] = new NamespaceProvider($"{@namespace}.{nextSegment}");
                    }
                }
                catch
                { }
            }

            return result;
        }
#endif
    }
}
