using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler;

namespace SharpJIT.Core
{
    /// <summary>
    /// Defines the virtual machine
    /// </summary>
    public class VirtualMachine : IDisposable
    {
        /// <summary>
        /// Returns the binder
        /// </summary>
        public Binder Binder { get; } = new Binder();

        /// <summary>
        /// Creates a new type provider
        /// </summary>
        public TypeProvider TypeProvider { get; } = new TypeProvider();

        /// <summary>
        /// Returns the compiler
        /// </summary>
        public IJITCompiler Compiler { get; }

        /// <summary>
        /// Returns the verifier
        /// </summary>
        public Verifier Verifier { get; }

        /// <summary>
        /// The settings for the VM
        /// </summary>
        public IDictionary<string, object> Settings { get; } = new Dictionary<string, object>();

        private readonly IList<Assembly> loadedAssemblies = new List<Assembly>();

        /// <summary>
        /// Creates a new virtual machine
        /// </summary>
        /// <param name="createCompilerFn">A function to create the compiler</param>
        public VirtualMachine(Func<VirtualMachine, IJITCompiler> createCompilerFn)
        {
            this.Compiler = createCompilerFn(this);
            this.Verifier = new Verifier(this);
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

            return (EntryPoint)Marshal.GetDelegateForFunctionPointer(
                entryPoint.EntryPoint,
                typeof(EntryPoint));
        }

        /// <summary>
        /// Returns the loaded assemblies
        /// </summary>
        public IReadOnlyList<Assembly> LoadedAssemblies
        {
            get { return new ReadOnlyCollection<Assembly>(this.loadedAssemblies); }
        }

        /// <summary>
        /// Loads the given assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public void LoadAssembly(Assembly assembly)
        {
            this.loadedAssemblies.Add(assembly);

            foreach (var function in assembly.Functions)
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
        /// Compiles loaded assemblies
        /// </summary>
        public void Compile()
        {
            foreach (var assembly in this.loadedAssemblies)
            {
                foreach (var function in assembly.Functions)
                {
                    this.Verifier.VerifiyFunction(function);
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
            this.Compiler.Dispose();
        }
    }
}
