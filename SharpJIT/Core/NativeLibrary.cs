using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
    /// <summary>
    /// Defines the functions that are not defined in managed code
    /// </summary>
    public static class NativeLibrary
    {
        /// <summary>
        /// Delegate for a '(Int) Void' function.
        /// </summary>
        delegate void FuncVoidArgInt(int x);

        /// <summary>
        /// Prints the given value
        /// </summary>
        /// <param name="value">The value</param>
        private static void Println(int value)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Delegate for a '(Float) Void' function.
        /// </summary>
        delegate void FuncVoidArgFloat(float x);

        /// <summary>
        /// Prints the given value
        /// </summary>
        /// <param name="value">The value</param>
        private static void Println(float value)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        /// Adds the native library to the given VM
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public static void Add(VirtualMachine virtualMachine)
        {
            var intType = virtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Int);
            var floatType = virtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Float);
            var voidType = virtualMachine.TypeProvider.GetPrimitiveType(PrimitiveTypes.Void);

            virtualMachine.Binder.Define(FunctionDefinition.NewExternal<FuncVoidArgInt>(
                "std.println",
                new List<VMType>() { intType },
                voidType,
                Println));

            virtualMachine.Binder.Define(FunctionDefinition.NewExternal<FuncVoidArgFloat>(
                "std.println",
                new List<VMType>() { floatType },
                voidType,
                Println));
        }
    }
}
