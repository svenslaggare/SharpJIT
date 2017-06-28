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
        /// <param name="typeProvider">The type provider</param>
        /// <param name="name">The name of the type</param>
        public static BaseType FindType(TypeProvider typeProvider, string name)
        {
            var type = typeProvider.FindType(name);
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
        /// <returns>The loaded classes</returns>
        IList<ClassMetadata> LoadClasses(IReadOnlyList<Data.Class> classes);
    }

    /// <summary>
    /// Represents a class loader
    /// </summary>
    public sealed class ClassLoader : IClassLoader
    {
        private readonly TypeProvider typeProvider;
        private readonly ClassMetadataProvider classMetadataProvider;

        /// <summary>
        /// Creates a new class loader
        /// </summary>
        /// <param name="typeProvider">The type provider</param>
        /// <param name="classMetadataProvider">The class metadata providert</param>
        public ClassLoader(TypeProvider typeProvider, ClassMetadataProvider classMetadataProvider)
        {
            this.typeProvider = typeProvider;
            this.classMetadataProvider = classMetadataProvider;
        }

        /// <summary>
        /// Defines the classes
        /// </summary>
        /// <param name="classes">The classes</param>
        private IList<ClassMetadata> DefineClasses(IReadOnlyList<Data.Class> classes)
        {
            var definedClasses = new List<ClassMetadata>();

            foreach (var currentClass in classes)
            {
                if (this.classMetadataProvider.IsDefined(currentClass.Name))
                {
                    throw new LoaderException($"The class '{currentClass.Name}' is already defined.");
                }

                var classMetadata = new ClassMetadata(currentClass.Name);
                this.classMetadataProvider.Add(classMetadata);
                definedClasses.Add(classMetadata);
            }

            return definedClasses;
        }

        /// <summary>
        /// Defines the fields
        /// </summary>
        /// <param name="classes">The classes</param>
        private void DefineFields(IReadOnlyList<Data.Class> classes)
        {
            foreach (var currentClass in classes)
            {
                var classMetadata = this.classMetadataProvider.GetMetadata(currentClass.Name);

                foreach (var field in currentClass.Fields)
                {
                    classMetadata.DefineField(new FieldDefinition(
                        field.Name,
                        LoaderHelpers.FindType(this.typeProvider, field.Type),
                        field.AccessModifier));
                }
            }
        }

        /// <summary>
        /// Creates the fields
        /// </summary>
        /// <param name="classes">The classes</param>
        private void CreateFields(IList<ClassMetadata> classes)
        {
            foreach (var classMetadata in classes)
            {
                classMetadata.CreateFields();
            }
        }

        /// <summary>
        /// Loads the given classes
        /// </summary>
        /// <param name="classes">The classes</param>
        public IList<ClassMetadata> LoadClasses(IReadOnlyList<Data.Class> classes)
        {
            var definedClasses = this.DefineClasses(classes);
            this.DefineFields(classes);
            this.CreateFields(definedClasses);
            return definedClasses;
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
        ManagedFunction LoadManagedFunction(Data.Function function, FunctionDefinition functionDefinition);
    }

    /// <summary>
    /// Represents a function loader
    /// </summary>
    public sealed class FunctionLoader : IFunctionLoader
    {
        private readonly TypeProvider typeProvider;

        /// <summary>
        /// Creates a new function loader
        /// </summary>
        /// <param name="typeProvider">The type provider</param>
        public FunctionLoader(TypeProvider typeProvider)
        {
            this.typeProvider = typeProvider;
        }

        /// <summary>
        /// Loads the given managed function
        /// </summary>
        /// <param name="function">The function</param>
        /// <param name="functionDefinition">The function definition</param>
        public ManagedFunction LoadManagedFunction(Data.Function function, FunctionDefinition functionDefinition)
        {
            var locals = function.Locals.Select(local => LoaderHelpers.FindType(this.typeProvider, local)).ToList();
            var instructions = function.Instructions.Select(instruction =>
            {
                switch (instruction.Format)
                {
                    case Data.InstructionFormat.OpCodeOnly:
                        return new Instruction(instruction.OpCode);
                    case Data.InstructionFormat.IntValue:
                        return new Instruction(instruction.OpCode, instruction.IntValue);
                    case Data.InstructionFormat.FloatValue:
                        return new Instruction(instruction.OpCode, instruction.FloatValue);
                    case Data.InstructionFormat.StringValue:
                        return new Instruction(instruction.OpCode, instruction.StringValue);
                    case Data.InstructionFormat.Call:
                        return new Instruction(
                            instruction.OpCode,
                            instruction.StringValue,
                            instruction.Parameters.Select(parameter => LoaderHelpers.FindType(this.typeProvider, parameter)).ToList());
                    case Data.InstructionFormat.CallInstance:
                        {
                            var classType = LoaderHelpers.FindType(this.typeProvider, TypeSystem.ClassTypeName(instruction.ClassType));
                            if (!classType.IsClass)
                            {
                                throw new LoaderException($"'{instruction.ClassType}' is not a class type.");
                            }

                            return new Instruction(
                                instruction.OpCode,
                                instruction.StringValue,
                                (ClassType)classType,
                                instruction.Parameters.Select(parameter => LoaderHelpers.FindType(this.typeProvider, parameter)).ToList());
                        }
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
        private readonly TypeProvider typeProvider;

        /// <summary>
        /// Creates a new assembly loader
        /// </summary>
        /// <param name="classLoader">The class loader</param>
        /// <param name="functionLoader">The function loader</param>
        /// <param name="typeProvider">The type provider</param>
        public AssemblyLoader(IClassLoader classLoader, IFunctionLoader functionLoader, TypeProvider typeProvider)
        {
            this.classLoader = classLoader;
            this.functionLoader = functionLoader;
            this.typeProvider = typeProvider;
        }

        /// <summary>
        /// Creates a definition for the given parsed function
        /// </summary>
        /// <param name="function">The function</param>
        private FunctionDefinition CreateFunctionDefinition(Data.Function function)
        {
            var parameters = function.Parameters.Select(parameterType => LoaderHelpers.FindType(this.typeProvider, parameterType)).ToList();
            var returnType = LoaderHelpers.FindType(this.typeProvider, function.ReturnType);

            if (function.ClassType == null)
            {
                return new FunctionDefinition(
                    function.Name,
                    parameters,
                    returnType);
            }
            else
            {
                var classType = LoaderHelpers.FindType(this.typeProvider, TypeSystem.ClassTypeName(function.ClassType));
                if (!classType.IsClass)
                {
                    throw new LoaderException($"'{function.ClassType}' is not a class type.");
                }

                return new FunctionDefinition(
                    function.Name,
                    parameters,
                    returnType,
                    (ClassType)classType,
                    function.IsConstructor);
            }
        }

        /// <summary>
        /// Loads the given assembly
        /// </summary>
        /// <param name="assembly">The assembly</param>
        public Assembly LoadAssembly(Data.Assembly assembly)
        {
            var loadedClasses = this.classLoader.LoadClasses(assembly.Classes);
            var loadedFunctions = assembly.Functions.Select(func =>
            {
                var funcDef = this.CreateFunctionDefinition(func);
                return this.functionLoader.LoadManagedFunction(func, funcDef);
            }).ToList();

            return new Assembly(assembly.Name, loadedClasses, loadedFunctions);
        }
    }
}
