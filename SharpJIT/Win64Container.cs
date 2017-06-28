using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Compiler.Win64;
using SharpJIT.Core;
using SharpJIT.Runtime;

namespace SharpJIT
{
    /// <summary>
    /// Represents container for a Windows x64 VM
    /// </summary>
    public class Win64Container : IDisposable
    {
        /// <summary>
        /// Returns the virtual machine
        /// </summary>
        public VirtualMachine VirtualMachine { get; }

        /// <summary>
        /// Creates a new Windows x64 container
        /// </summary>
        /// <param name="config">The configuration</param>
        public Win64Container(VirtualMachineConfiguration config = null)
        {
            this.VirtualMachine = new VirtualMachine(config ?? new VirtualMachineConfiguration(), vm => new JITCompiler(vm));
            NativeLibrary.Add(this.VirtualMachine);
            RuntimeInterface.Initialize(this.VirtualMachine);
        }

        /// <summary>
        /// Loads the given assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public void LoadAssembly(Loader.Data.Assembly assembly)
        {
            this.VirtualMachine.LoadAssembly(assembly);
        }

        /// <summary>
        /// Executes the loaded program
        /// </summary>
        /// <returns>The return value from the program</returns>
        public int Execute()
        {
            this.VirtualMachine.Compile();
            var funcPtr = this.VirtualMachine.GetEntryPoint();
            return funcPtr();
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            this.VirtualMachine.Dispose();
        }
    }
}
