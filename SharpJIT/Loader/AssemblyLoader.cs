using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Core;
using SharpJIT.Core.Objects;
using SharpJIT.Runtime;

namespace SharpJIT.Loader
{
    /// <summary>
    /// Represents a loader exception
    /// </summary>
    public class LoaderException : Exception
    {
        /// <summary>
        /// Represents a loader exception
        /// </summary>
        /// <param name="message">The message</param>
        public LoaderException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// Contains helper methods for loader classes
    /// </summary>
    public static class LoaderHelpers
    {
        /// <summary>
        /// Finds the given type
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        /// <param name="name">The name of the type</param>
        public static BaseType FindType(VirtualMachine virtualMachine, string name)
        {
            var type = virtualMachine.TypeProvider.FindType(name);
            if (type == null)
            {
                throw new LoaderException($"There exist no type called '{name}'.");
            }

            return type;
        }
    }

    /// <summary>
    /// Represents a class loader
    /// </summary>
    public interface IClassLoader
    {
        /// <summary>
        /// Loads the given classes
        /// </summary>
        /// <param name="classes">The classes</param>
        void LoadClasses(IReadOnlyList<Parser.Class> classes);
    }

    /// <summary>
    /// Represents a class loader
    /// </summary>
    public sealed class ClassLoader : IClassLoader
    {
        private readonly VirtualMachine virtualMachine;

        /// <summary>
        /// Creates a new class loader
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public ClassLoader(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;

        }

        /// <summary>
        /// Defines the classes
        /// </summary>
        /// <param name="classes">The classes</param>
        private void DefineClasses(IReadOnlyList<Parser.Class> classes)
        {
            foreach (var currentClass in classes)
            {
                if (this.virtualMachine.ClassMetadataProvider.IsDefined(currentClass.Name))
                {
                    throw new LoaderException($"The class '{currentClass.Name}' is already defined.");
                }

                this.virtualMachine.ClassMetadataProvider.Add(new ClassMetadata(currentClass.Name));
            }
        }

        /// <summary>
        /// Defines the fields
        /// </summary>
        /// <param name="classes">The classes</param>
        private void DefineFields(IReadOnlyList<Parser.Class> classes)
        {
            foreach (var currentClass in classes)
            {
                var classMetadata = this.virtualMachine.ClassMetadataProvider.GetMetadata(currentClass.Name);

                foreach (var field in currentClass.Fields)
                {
                    classMetadata.DefineField(new FieldDefinition(
                        field.Name,
                        LoaderHelpers.FindType(this.virtualMachine, field.Type),
                        field.AccessModifier));
                }
            }
        }

        /// <summary>
        /// Creates the fields
        /// </summary>
        /// <param name="classes">The classes</param>
        private void CreateFields(IReadOnlyList<Parser.Class> classes)
        {
            foreach (var currentClass in classes)
            {
                var classMetadata = this.virtualMachine.ClassMetadataProvider.GetMetadata(currentClass.Name);
                classMetadata.CreateFields();
            }
        }

        /// <summary>
        /// Loads the given classes
        /// </summary>
        /// <param name="classes">The classes</param>
        public void LoadClasses(IReadOnlyList<Parser.Class> classes)
        {
            this.DefineClasses(classes);
            this.DefineFields(classes);
            this.CreateFields(classes);
        }
    }

    /// <summary>
    /// Represents a function loader
    /// </summary>
    public interface IFunctionLoader
    {
        /// <summary>
        /// Loads the given managed function
        /// </summary>
        /// <param name="function">The function</param>
        /// <param name="functionDefinition">The function definition</param>
        ManagedFunction LoadManagedFunction(Parser.Function function, FunctionDefinition functionDefinition);
    }

    /// <summary>
    /// Represents a function loader
    /// </summary>
    public sealed class FunctionLoader : IFunctionLoader
    {
        private readonly VirtualMachine virtualMachine;

        /// <summary>
        /// Creates a new function loader
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public FunctionLoader(VirtualMachine virtualMachine)
        {
            this.virtualMachine = virtualMachine;
        }

        /// <summary>
        /// Loads the given managed function
        /// </summary>
        /// <param name="function">The function</param>
        /// <param name="functionDefinition">The function definition</param>
        public ManagedFunction LoadManagedFunction(Parser.Function function, FunctionDefinition functionDefinition)
        {
            var locals = function.Locals.Select(local => LoaderHelpers.FindType(this.virtualMachine, local)).ToList();
            var instructions = function.Instructions.Select(instruction =>
            {
                switch (instruction.Format)
                {
                    case Parser.InstructionFormat.OpCodeOnly:
                        return new Instruction(instruction.OpCode);
                    case Parser.InstructionFormat.IntValue:
                        return new Instruction(instruction.OpCode, instruction.IntValue);
                    case Parser.InstructionFormat.FloatValue:
                        return new Instruction(instruction.OpCode, instruction.FloatValue);
                    case Parser.InstructionFormat.StringValue:
                        return new Instruction(instruction.OpCode, instruction.StringValue);
                    case Parser.InstructionFormat.Call:
                        return new Instruction(
                            instruction.OpCode,
                            instruction.StringValue,
                            instruction.Parameters.Select(parameter => LoaderHelpers.FindType(this.virtualMachine, parameter)).ToList());
                    default:
                        return new Instruction();
                }
            }).ToList();

            return new ManagedFunction(functionDefinition, locals, instructions);
        }
    }

    /// <summary>
    /// Represents an assembly loader
    /// </summary>
    public sealed class AssemblyLoader
    {
        private readonly IClassLoader classLoader;
        private readonly IFunctionLoader functionLoader;
        private readonly VirtualMachine virtualMachine;

        /// <summary>
        /// Creates a new assembly loader
        /// </summary>
        /// <param name="classLoader">The class loader</param>
        /// <param name="functionLoader">The function loader</param>
        /// <param name="virtualMachine">The virtual machine</param>
        public AssemblyLoader(IClassLoader classLoader, IFunctionLoader functionLoader, VirtualMachine virtualMachine)
        {
            this.classLoader = classLoader;
            this.functionLoader = functionLoader;
            this.virtualMachine = virtualMachine;
        }

        /// <summary>
        /// Creates a definition for the given parsed function
        /// </summary>
        /// <param name="function">The function</param>
        private FunctionDefinition CreateFunctionDefinition(Parser.Function function)
        {
            return new FunctionDefinition(
                function.Name,
                function.Parameters.Select(parameterType => LoaderHelpers.FindType(this.virtualMachine, parameterType)).ToList(),
                LoaderHelpers.FindType(this.virtualMachine, function.ReturnType));
        }

        /// <summary>
        /// Loads the given assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public Assembly LoadAssembly(Parser.Assembly assembly)
        {
            this.classLoader.LoadClasses(assembly.Classes);
            var loadedFunctions = assembly.Functions.Select(func =>
            {
                var funcDef = this.CreateFunctionDefinition(func);
                return this.functionLoader.LoadManagedFunction(func, funcDef);
            }).ToList();

            return new Assembly(assembly.Name, loadedFunctions);
        }
    }
}
