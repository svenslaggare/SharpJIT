using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Loader;
using SharpJIT.Runtime.Frame;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Runtime
{
    /// <summary>
    /// The configuration for the virtual machine
    /// </summary>
    public sealed class VirtualMachineConfiguration
    {
        /// <summary>
        /// Indicates if debug is enabled
        /// </summary>
        public bool EnableDebug { get; }

        /// <summary>
        /// Prints alive objects when GC
        /// </summary>
        public bool PrintAliveObjectsWhenGC { get; }

        /// <summary>
        /// Prints the stack frame when GC
        /// </summary>
        public bool PrintStackFrameWhenGC { get; }

        /// <summary>
        /// Prints allocation
        /// </summary>
        public bool PrintAllocation { get; }

        /// <summary>
        /// Prints deallocations
        /// </summary>
        public bool PrintDeallocation { get; }

        /// <summary>
        /// Indicates if allocations are logged
        /// </summary>
        public bool LogAllocation { get; }

        /// <summary>
        /// Indicates if deallocations are logged
        /// </summary>
        public bool LogDeallocation { get; }

        /// <summary>
        /// Creates a new config
        /// </summary>
        /// <param name="enableDebug">Indicates if debug is enabled</param>
        /// <param name="printAliveObjectsWhenGC">Prints alive objects when GC</param>
        /// <param name="printStackFrameWhenGC">Prints the stack frame when GC</param>
        /// <param name="printAllocation">Prints allocation</param>
        /// <param name="printDeallocation">Prints deallocations</param>
        /// <param name="logAllocation">Indicates if allocations are logged</param>
        /// <param name="logDeallocation">Indicates if deallocations are logged</param>
        public VirtualMachineConfiguration(
            bool enableDebug = false,
            bool printAliveObjectsWhenGC = false,
            bool printStackFrameWhenGC = false,
            bool printAllocation = false,
            bool printDeallocation = false,
            bool logAllocation = false,
            bool logDeallocation = false)
        {
            this.EnableDebug = enableDebug;
            this.PrintAliveObjectsWhenGC = printAliveObjectsWhenGC;
            this.PrintStackFrameWhenGC = printAliveObjectsWhenGC;
            this.PrintAllocation = printAllocation;
            this.PrintDeallocation = printDeallocation;
            this.LogAllocation = logAllocation;
            this.LogDeallocation = logDeallocation;
        }
    }

    /// <summary>
    /// Defines the virtual machine
    /// </summary>
    public sealed class VirtualMachine : IDisposable
    {
        /// <summary>
        /// The configuration
        /// </summary>
        public VirtualMachineConfiguration Config { get; }

        /// <summary>
        /// The binder
        /// </summary>
        public FunctionBinder Binder { get; } = new FunctionBinder();

        /// <summary>
        /// The class metadata provider
        /// </summary>
        public ClassMetadataProvider ClassMetadataProvider { get; } = new ClassMetadataProvider();

        /// <summary>
        /// The type provider
        /// </summary>
        public TypeProvider TypeProvider { get; }

        /// <summary>
        /// The verifier
        /// </summary>
        public Verifier Verifier { get; }

        private readonly AssemblyLoader assemblyLoader;

        /// <summary>
        /// The object references
        /// </summary>
        public ManagedObjectReferences ManagedObjectReferences { get; } = new ManagedObjectReferences();

        /// <summary>
        /// The memory manager
        /// </summary>
        public MemoryManager MemoryManager { get; } = new MemoryManager();

        /// <summary>
        /// The call stack
        /// </summary>
        public CallStack CallStack { get; }

        /// <summary>
        /// The compiler
        /// </summary>
        public IJITCompiler Compiler { get; }

        /// <summary>
        /// The garbage collector
        /// </summary>
        public GarbageCollector GarbageCollector { get; }

        private readonly IList<Assembly> loadedAssemblies = new List<Assembly>();

        /// <summary>
        /// Creates a new virtual machine
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <param name="createCompilerFn">A function to create the compiler</param>
        public VirtualMachine(VirtualMachineConfiguration config, Func<VirtualMachine, IJITCompiler> createCompilerFn)
        {
            this.Config = config;

            this.TypeProvider = new TypeProvider(this.ClassMetadataProvider);
            this.Verifier = new Verifier(this);
            this.assemblyLoader = new AssemblyLoader(
                new ClassLoader(this.TypeProvider, this.ClassMetadataProvider),
                new FunctionLoader(this.TypeProvider),
                this.TypeProvider);

            this.CallStack = new CallStack(this.MemoryManager, this.ManagedObjectReferences, 5000);
            this.Compiler = createCompilerFn(this);
            this.GarbageCollector = new GarbageCollector(this);
        }

        /// <summary>
        /// Returns the entry point
        /// </summary>
        public EntryPoint GetEntryPoint()
        {
            var entryPoint = this.Binder.GetFunction("main()");

            if (entryPoint == null)
            {
                throw new InvalidOperationException("There is no entry point defined.");
            }

            return Marshal.GetDelegateForFunctionPointer<EntryPoint>(entryPoint.EntryPoint);
        }

        /// <summary>
        /// Returns the loaded assemblies
        /// </summary>
        public IReadOnlyList<Assembly> LoadedAssemblies
        {
            get { return new ReadOnlyCollection<Assembly>(this.loadedAssemblies); }
        }

        /// <summary>
        /// Loads the given functions
        /// </summary>
        /// <param name="functions">The functions</param>
        private void LoadFunctions(IEnumerable<ManagedFunction> functions)
        {
            foreach (var function in functions)
            {
                if (function.Definition.Name == "main")
                {
                    if (!(function.Definition.Parameters.Count == 0
                          && function.Definition.ReturnType.IsPrimitiveType(PrimitiveTypes.Int)))
                    {
                        throw new Exception("Expected the main function to have the signature: 'main() Int'.");
                    }
                }

                if (!this.Binder.Define(function.Definition))
                {
                    throw new Exception($"The function '{function.Definition}' is already defined.");
                }
            }
        }

        /// <summary>
        /// Loads the given assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public void LoadAssembly(Loader.Data.Assembly assembly)
        {
            var loadedAssembly = this.assemblyLoader.LoadAssembly(assembly);
            this.LoadFunctions(loadedAssembly.Functions);
            this.loadedAssemblies.Add(loadedAssembly);
        }

        /// <summary>
        /// Loads the given functions as an assembly
        /// </summary>
        /// <param name="functions">The functions</param>
        public void LoadFunctionsAsAssembly(IList<ManagedFunction> functions)
        {
            var assembly = new Assembly(
                Guid.NewGuid().ToString(),
                new List<ClassMetadata>(),
                functions);
            this.LoadFunctions(functions);
            this.loadedAssemblies.Add(assembly);
        }

        /// <summary>
        /// Compiles loaded assemblies
        /// </summary>
        public void Compile()
        {
            foreach (var assembly in this.loadedAssemblies)
            {
                foreach (var function in assembly.Functions)
                {
                    this.Verifier.VerifyFunction(function);
                    this.Compiler.Compile(function);
                }
            }

            this.Compiler.MakeExecutable();
        }

        /// <summary>
        /// Disposes the resources
        /// </summary>
        public void Dispose()
        {
            this.MemoryManager.Dispose();
        }
    }
}
