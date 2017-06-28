using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Core
{
	/// <summary>
	/// The op-codes for the instructions
	/// </summary>
	public enum OpCodes : byte
	{
        Pop,
        LoadInt,
        LoadFloat,
		AddInt,
		SubInt,
		MulInt,
		DivInt,
        AddFloat,
        SubFloat,
        MulFloat,
        DivFloat,
        LoadTrue,
        LoadFalse,
        And,
        Or,
        Not,
        Call,
        Return,
        LoadArgument,
        LoadLocal,
        StoreLocal,
        Branch,
        BranchEqual,
        BranchNotEqual,
        BranchGreaterThan,
        BranchGreaterThanOrEqual,
        BranchLessThan,
        BranchLessOrEqual,
        CompareEqual,
        CompareNotEqual,
        CompareGreaterThan,
        CompareGreaterThanOrEqual,
        CompareLessThan,
        CompareLessThanOrEqual,
        LoadNull,
        NewArray,
        LoadArrayLength,
        LoadElement,
        StoreElement,
        NewObject,
        LoadField,
        StoreField,
        CallInstance
    }

	/// <summary>
	/// Represents an instruction
	/// </summary>
	public struct Instruction
	{
        private readonly string stringRepresentation;
        private readonly string disassembledInstruction;

        /// <summary>
        /// Returns the op.code
        /// </summary>
		public OpCodes OpCode { get; }

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
        public IReadOnlyList<BaseType> Parameters { get; }

        /// <summary>
        /// Returns the class type for object instructions
        /// </summary>
        public ClassType ClassType { get; }

        private readonly static IReadOnlyDictionary<OpCodes, string> opCodeNames;

        static Instruction()
        {
            var opCodeNames = new Dictionary<OpCodes, string>()
            {
                { OpCodes.Pop, "pop" },
                { OpCodes.AddInt, "addint" },
                { OpCodes.SubInt, "subint" },
                { OpCodes.MulInt, "mulint" },
                { OpCodes.DivInt, "divint" },
                { OpCodes.AddFloat, "addfloat" },
                { OpCodes.SubFloat, "subfloat" },
                { OpCodes.MulFloat, "mulfloat" },
                { OpCodes.DivFloat, "divfloat" },
                { OpCodes.LoadInt, "ldint" },
                { OpCodes.LoadFloat, "ldfloat" },
                { OpCodes.LoadTrue, "ldtrue" },
                { OpCodes.LoadFalse, "ldfalse" },
                { OpCodes.And, "and" },
                { OpCodes.Or, "or" },
                { OpCodes.Not, "not" },
                { OpCodes.StoreLocal, "stloc" },
                { OpCodes.LoadLocal, "ldloc" },
                { OpCodes.LoadArgument, "ldarg" },
                { OpCodes.Call, "call" },
                { OpCodes.Return, "ret"  },
                { OpCodes.Branch, "br" },
                { OpCodes.BranchEqual, "beq" },
                { OpCodes.BranchNotEqual, "bne" },
                { OpCodes.BranchGreaterThan, "bgt" },
                { OpCodes.BranchGreaterThanOrEqual, "bge" },
                { OpCodes.BranchLessThan, "blt"  },
                { OpCodes.BranchLessOrEqual, "ble" },
                { OpCodes.CompareEqual, "cmpeq" },
                { OpCodes.CompareNotEqual, "cmpne" },
                { OpCodes.CompareGreaterThan, "cmpgt" },
                { OpCodes.CompareGreaterThanOrEqual, "cmpge" },
                { OpCodes.CompareLessThan, "cmplt"  },
                { OpCodes.CompareLessThanOrEqual, "cmple" },
                { OpCodes.LoadNull, "loadnull" },
                { OpCodes.NewArray, "newarr" },
                { OpCodes.LoadArrayLength, "ldlen" },
                { OpCodes.LoadElement, "ldelem" },
                { OpCodes.StoreElement, "stelem" },
                { OpCodes.NewObject, "newobj" },
                { OpCodes.LoadField, "ldfield" },
                { OpCodes.StoreField, "stfield" },
                { OpCodes.CallInstance, "callinst" },
            };

            Instruction.opCodeNames = new ReadOnlyDictionary<OpCodes, string>(opCodeNames);
        }

        /// <summary>
        /// Creates a new instruction
        /// </summary>
        /// <param name="opCode">The op code</param>
        public Instruction(OpCodes opCode)
		{
			this.OpCode = opCode;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
            this.stringRepresentation = $"OpCode: {opCode}";
            this.disassembledInstruction = opCodeNames[opCode].ToUpper();
        }

		/// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
		public Instruction(OpCodes opCode, int value)
		{
			this.OpCode = opCode;
            this.IntValue = value;
            this.FloatValue = 0.0f;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
            this.stringRepresentation = $"OpCode: {opCode}, IntValue: {value}";
            this.disassembledInstruction = $"{opCodeNames[opCode].ToUpper()} {value}";
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
		public Instruction(OpCodes opCode, float value)
        {
            this.OpCode = opCode;
            this.IntValue = 0;
            this.FloatValue = value;
            this.StringValue = null;
            this.Parameters = null;
            this.ClassType = null;
            this.stringRepresentation = $"OpCode: {opCode}, FloatValue: {value}";
            this.disassembledInstruction = $"{opCodeNames[opCode].ToUpper()} {value}";
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
		public Instruction(OpCodes opCode, string value)
        {
            this.OpCode = opCode;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.Parameters = null;
            this.ClassType = null;
            this.stringRepresentation = $"OpCode: {opCode}, StringValue: {value}";
            this.disassembledInstruction = $"{opCodeNames[opCode].ToUpper()} \"{value}\"";
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
        /// <param name="parameters">The parameters</param>
		public Instruction(OpCodes opCode, string value, IList<BaseType> parameters)
        {
            this.OpCode = opCode;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.ClassType = null;
            this.Parameters = new ReadOnlyCollection<BaseType>(new List<BaseType>(parameters));
            this.stringRepresentation = $"OpCode: {opCode}, StringValue: {value}, Parameters: {string.Join(" ", parameters)}";
            this.disassembledInstruction = $"{opCodeNames[opCode].ToUpper()} {value}({string.Join(" ", parameters)})";
        }

        /// <summary>
		/// Creates a new instruction
		/// </summary>
		/// <param name="opCode">The op-code</param>
		/// <param name="value">The value</param>
        /// <param name="classType">The class type</param>
        /// <param name="parameters">The parameters</param>
		public Instruction(OpCodes opCode, string value, ClassType classType, IList<BaseType> parameters)
        {
            this.OpCode = opCode;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
            this.StringValue = value;
            this.ClassType = classType;
            this.Parameters = new ReadOnlyCollection<BaseType>(new List<BaseType>(parameters));
            this.stringRepresentation = $"OpCode: {opCode}, StringValue: {value}, ClassType: {classType.ClassName}, Parameters: {string.Join(" ", parameters)}";
            this.disassembledInstruction = $"{opCodeNames[opCode].ToUpper()} {classType.Name}::{value}({string.Join(" ", parameters)})";
        }

        public override string ToString()
        {
            return this.stringRepresentation;
        }

        /// <summary>
        /// Disassembles the current instruction
        /// </summary>
        public string Disassemble()
        {
            return this.disassembledInstruction;
        }
    }

    /// <summary>
    /// Contains helper methods for instructions
    /// </summary>
    public static class InstructionHelpers
    {
        /// <summary>
        /// Indicates if the given instruction is a conditional branch
        /// </summary>
        /// <param name="instruction">The instruction</param>
        public static bool IsConditionalBranch(Instruction instruction)
        {
            return instruction.OpCode == OpCodes.BranchEqual
                    || instruction.OpCode == OpCodes.BranchNotEqual
                    || instruction.OpCode == OpCodes.BranchGreaterThan
                    || instruction.OpCode == OpCodes.BranchGreaterThanOrEqual
                    || instruction.OpCode == OpCodes.BranchLessThan
                    || instruction.OpCode == OpCodes.BranchLessOrEqual;
        }
    }
}
