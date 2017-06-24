using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Loader.Data
{
    /// <summary>
    /// Represents an assembly used for loading
    /// </summary>
    public sealed class Assembly
    {
        /// <summary>
        /// The name of the assembly
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The classes
        /// </summary>
        public IReadOnlyList<Class> Classes { get; }

        /// <summary>
        /// The functions
        /// </summary>
        public IReadOnlyList<Function> Functions { get; }

        /// <summary>
        /// Creates a new assembly
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="classes">The classes</param>
        /// <param name="functions">The functions</param>
        public Assembly(string name, IList<Class> classes, IList<Function> functions)
        {
            this.Name = name;
            this.Classes = new List<Class>(classes);
            this.Functions = new List<Function>(functions);
        }
    }

    /// <summary>
    /// Represents a function used for loading
    /// </summary>
    public sealed class Function
    {
        /// <summary>
        /// The name of the function
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The parameters of the function
        /// </summary>
        public IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// The return type
        /// </summary>
        public string ReturnType { get; }
        
        /// <summary>
        /// The class type if member function
        /// </summary>
        public string ClassType { get; }

        /// <summary>
        /// Indicates if the current function is a constructor
        /// </summary>
        public bool IsConstructor { get; }

        /// <summary>
        /// The type of the locals
        /// </summary>
        public IReadOnlyList<string> Locals { get; }

        /// <summary>
        /// The instructions
        /// </summary>
        public IReadOnlyList<Instruction> Instructions { get; }

        /// <summary>
        /// Creates a new managed function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        /// <param name="locals">The type of the locals</param>
        /// <param name="instructions">The instructions</param>
        public Function(string name, IList<string> parameters, string returnType, IList<string> locals, IList<Instruction> instructions)
        {
            this.Name = name;
            this.Parameters = new ReadOnlyCollection<string>(new List<string>(parameters));
            this.ReturnType = returnType;
            this.ClassType = null;
            this.IsConstructor = false;

            this.Locals = new ReadOnlyCollection<string>(new List<string>(locals));
            this.Instructions = new ReadOnlyCollection<Instruction>(new List<Instruction>(instructions));
        }

        /// <summary>
        /// Creates a new managed member function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        /// <param name="isConstructor">Indicates if the function is a constructor</param>
        /// <param name="locals">The type of the locals</param>
        /// <param name="instructions">The instructions</param>
        public Function(string name, IList<string> parameters, string returnType, string classType, bool isConstructor, IList<string> locals, IList<Instruction> instructions)
        {
            this.Name = name;
            this.Parameters = new ReadOnlyCollection<string>(new List<string>(parameters));
            this.ReturnType = returnType;
            this.ClassType = classType;
            this.IsConstructor = isConstructor;

            this.Locals = new ReadOnlyCollection<string>(new List<string>(locals));
            this.Instructions = new ReadOnlyCollection<Instruction>(new List<Instruction>(instructions));
        }

        /// <summary>
        /// Creates a new external function
        /// </summary>
        /// <param name="name">The name of the function</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="returnType">The return type</param>
        public Function(string name, IList<string> parameters, string returnType)
        {
            this.Name = name;
            this.Parameters = new ReadOnlyCollection<string>(new List<string>(parameters));
            this.ReturnType = returnType;
        }
    }

    /// <summary>
    /// The instruction formats
    /// </summary>
    public enum InstructionFormat
    {
        OpCodeOnly,
        IntValue,
        FloatValue,
        StringValue,
        Call,
        CallInstance
    }

    /// <summary>
    /// Represents an instruction
    /// </summary>
    public struct Instruction
    {
        /// Returns the op.code
        /// </summary>
        public Core.OpCodes OpCode { get; }

        /// <summary>
        /// The instruction format
        /// </summary>
        public InstructionFormat Format { get; }

        /// <summary>
        /// Returns the int value
        /// </summary>
        public int IntValue { get; }

        /// <summary>
        /// Returns the float value
        /// </summary>
        public float FloatValue { get; }

        /// <summary>
        /// Returns the string value
        /// </summary>
        public string StringValue { get; }

        /// <summary>
        /// Returns the parameters used for call instructions 
        /// </summary>
        public IReadOnlyList<string> Parameters { get; }

        /// <summary>
        /// Returns the class type user for object instrctions
        /// </summary>
        public string ClassType { get; }

        /// <summary>
        /// Creates a new instruction
        /// </summary>
        /// <param name="opCode">The op code</param>
        public Instruction(Core.OpCodes opCode)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.OpCodeOnly;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
        }

        /// <summary>
        /// Creates a new instruction
        /// </summary>
        /// <param name="opCode">The op-code</param>
        /// <param name="value">The value</param>
        public Instruction(Core.OpCodes opCode, int value)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.IntValue;
            this.IntValue = value;
            this.FloatValue = 0.0f;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
		public Instruction(Core.OpCodes opCode, float value)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.FloatValue;
            this.IntValue = 0;
            this.FloatValue = value;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
		public Instruction(Core.OpCodes opCode, string value)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.StringValue;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.Parameters = null;
            this.ClassType = null;
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
        /// <param name="parameters">The parameters</param>
		public Instruction(Core.OpCodes opCode, string value, IList<string> parameters)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.Call;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.Parameters = new ReadOnlyCollection<string>(new List<string>(parameters));
            this.ClassType = null;
        }


        /// <summary>
        /// Creates a new instruction
        /// </summary>
        /// <param name="opCode">The op-code</param>
        /// <param name="value">The value</param>
        /// <param name="classType">The class type</param>
        /// <param name="parameters">The parameters</param>
        public Instruction(Core.OpCodes opCode, string value, string classType, IList<string> parameters)
        {
            this.OpCode = opCode;
            this.Format = InstructionFormat.CallInstance;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.ClassType = classType;
            this.Parameters = new ReadOnlyCollection<string>(new List<string>(parameters));
        }
    }

    /// <summary>
    /// Represents a field for loading
    /// </summary>
    public sealed class Field
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the field
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The access modifier
        /// </summary>
        public Core.Objects.AccessModifier AccessModifier { get; }

        /// <summary>
        /// Creates a new field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <param name="type">The type of the field</param>
        /// <param name="accessModifier">The access modifier</param>
        public Field(string name, string type, Core.Objects.AccessModifier accessModifier)
        {
            this.Name = name;
            this.Type = type;
            this.AccessModifier = accessModifier;
        }
    }

    /// <summary>
    /// Represents a class used for loading
    /// </summary>
    public sealed class Class
    {
        /// <summary>
        /// The name of the class
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The fields
        /// </summary>
        public IReadOnlyList<Field> Fields { get; }

        /// <summary>
        /// Represents a class
        /// </summary>
        /// <param name="name">The name of the class</param>
        /// <param name="fields">The fields</param>
        public Class(string name, IList<Field> fields)
        {
            this.Name = name;
            this.Fields = new ReadOnlyCollection<Field>(new List<Field>(fields));
        }
    }
}
