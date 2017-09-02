using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;

namespace SharpJIT.Runtime
{
    /// <summary>
    /// Defines the functions that are not defined in managed code
    /// </summary>
    public static class NativeLibrary
    {
        /// <summary>
        /// Delegate for a 'Fn(Int) Void' function.
        /// </summary>
        delegate void FuncVoidArgInt(int x);

        /// <summary>
        /// Delegate for a 'Fn(Float) Void' function.
        /// </summary>
        delegate void FuncVoidArgFloat(float x);

        /// <summary>
        /// Delegate for a 'Fn(Ref.Array[Int]) Void' function.
        /// </summary>
        delegate void FuncVoidArgArrayRef(long arrayRef);

        /// <summary>
        /// Prints the given value
        /// </summary>
        /// <param name="value">The value</param>
        private static void Println(int value)
        {
            Console.WriteLine(value);
        }

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
            var intType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Int);
            var floatType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Float);
            var voidType = virtualMachine.TypeProvider.FindPrimitiveType(PrimitiveTypes.Void);

            virtualMachine.Binder.Define(FunctionDefinition.NewExternal<FuncVoidArgInt>(
                "std.println",
                new List<BaseType>() { intType },
                voidType,
                Println));

            virtualMachine.Binder.Define(FunctionDefinition.NewExternal<FuncVoidArgFloat>(
                "std.println",
                new List<BaseType>() { floatType },
                voidType,
                Println));
        }
    }
}
