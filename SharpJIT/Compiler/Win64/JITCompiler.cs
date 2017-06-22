using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;
using SharpJIT.Runtime;
using SharpJIT.Runtime.Memory;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Represents a JIT compiler
    /// </summary>
    public sealed class JITCompiler : IJITCompiler
    {
        private readonly VirtualMachine virtualMachine;
        private readonly CodeGenerator codeGenerator;
        private readonly IDictionary<ManagedFunction, CompilationData> compiledFunctions = new Dictionary<ManagedFunction, CompilationData>();

        /// <summary>
        /// Creates a new compiler
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public JITCompiler(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
            this.codeGenerator = new CodeGenerator(virtualMachine);
        }

        /// <summary>
        /// Returns the memory manager
        /// </summary>
        private MemoryManager MemoryManager
        {
            get
            {
                return this.virtualMachine.MemoryManager;
            }
        }

        /// <summary>
        /// Returns the compilation data for the given function
        /// </summary>
        /// <param name="function">The function</param>
        /// <returns>The data or null if not compiled</returns>
        public AbstractCompilationData GetCompilationData(ManagedFunction function)
        {
            if (this.compiledFunctions.TryGetValue(function, out var compilationData))
            {
                return compilationData;
            }

            return null;
        }

        /// <summary>
        /// Compiles the given function
        /// </summary>
        /// <param name="function">The function to compile</param>
        /// <returns>A pointer to the start of the compiled function</returns>
        public IntPtr Compile(ManagedFunction function)
        {
            //Compile the function
            var compilationData = new CompilationData(function);
            this.compiledFunctions.Add(function, compilationData);
            this.codeGenerator.CompileFunction(compilationData);

            //Allocate native memory. The instructions will be copied later when all symbols has been resolved.
            var memory = this.MemoryManager.AllocateCode(function.GeneratedCode.Count);
            function.Definition.SetEntryPoint(memory);

            return memory;
        }

        /// <summary>
        /// Resolves the branches for the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void ResolveBranches(CompilationData compilationData)
        {
            foreach (var branch in compilationData.UnresolvedBranches)
            {
                int source = branch.Key;
                var branchTarget = branch.Value;

                int nativeTarget = compilationData.InstructionMapping[branchTarget.Target];

                //Calculate the native jump location
                int target = nativeTarget - source - branchTarget.InstructionSize;

                //Update the source with the native target
                int sourceOffset = source + branchTarget.InstructionSize - sizeof(int);
                NativeHelpers.SetInt(compilationData.Function.GeneratedCode, sourceOffset, target);
            }

            compilationData.UnresolvedBranches.Clear();
        }

        /// <summary>
        /// Resolves the call target for the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void ResolveCallTargets(CompilationData compilationData)
        {
            var generatedCode = compilationData.Function.GeneratedCode;
            var entryPoint = compilationData.Function.Definition.EntryPoint.ToInt64();

            foreach (var unresolvedCall in compilationData.UnresolvedFunctionCalls)
            {
                var toCallAddress = unresolvedCall.Function.EntryPoint.ToInt64();

                //Update the call target
                if (unresolvedCall.AddressMode == FunctionCallAddressMode.Absolute)
                {
                    NativeHelpers.SetLong(generatedCode, unresolvedCall.CallSiteOffset + 2, toCallAddress);
                }
                else
                {
                    int target = (int)(toCallAddress - (entryPoint + unresolvedCall.CallSiteOffset + 5));
                    NativeHelpers.SetInt(generatedCode, unresolvedCall.CallSiteOffset + 1, target);
                }
            }

            compilationData.UnresolvedFunctionCalls.Clear();
        }

        /// <summary>
        /// Resolves the native labels for the given function
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        private void ResolveNativeLabels(CompilationData compilationData)
        {
            var generatedCode = compilationData.Function.GeneratedCode;
            var entryPoint = compilationData.Function.Definition.EntryPoint.ToInt64();

            foreach (var nativeLabel in compilationData.UnresolvedNativeLabels)
            {
                var source = nativeLabel.Key;
                var target = nativeLabel.Value.ToInt64();

                //Calculate the native jump location
                var nativeTarget = (int)(target - (entryPoint + source) - 6);

                //Update the source with the native target
                var sourceOffset = source + 6 - sizeof(int);
                NativeHelpers.SetInt(generatedCode, sourceOffset, nativeTarget);
            }

            compilationData.UnresolvedNativeLabels.Clear();
        }

        /// <summary>
        /// Resolve the symbols for functions
        /// </summary>
        private void ResolveSymbols()
        {
            foreach (var function in this.compiledFunctions.Values)
            {
                this.ResolveCallTargets(function);
                this.ResolveBranches(function);
                this.ResolveNativeLabels(function);
                NativeHelpers.CopyTo(
                    function.Function.Definition.EntryPoint,
                    function.Function.GeneratedCode);
            }
        }

        /// <summary>
        /// Makes the compiled functions executable
        /// </summary>
        public void MakeExecutable()
        {
            this.ResolveSymbols();
            this.MemoryManager.MakeExecutable();
        }
    }
}
