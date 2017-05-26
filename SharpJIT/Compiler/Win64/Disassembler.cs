using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaEngine.Net;

namespace SharpJIT.Compiler.Win64
{
    /// <summary>
    /// Represents a disassembler
    /// </summary>
    public class Disassembler : INativeDisassembler
    {
        private readonly AbstractCompilationData compilationData;
        private readonly Disasm disassembler;
        private readonly UnmanagedBuffer codeBuffer;

        /// <summary>
        /// Creates a new disassembler
        /// </summary>
        /// <param name="compilationData">The compilation data</param>
        public Disassembler(AbstractCompilationData compilationData)
        {
            this.compilationData = compilationData;

            this.codeBuffer = new UnmanagedBuffer(compilationData.Function.GeneratedCode.ToArray());
            this.disassembler = new Disasm()
            {
                Archi = 64,
                EIP = new IntPtr(this.codeBuffer.Ptr.ToInt64())
            };
        }

        /// <summary>
        /// Disassembles the given code
        /// </summary>
        /// <param name="generatedCode">The generated code</param>
        public static string Disassemble(IList<byte> generatedCode)
        {
            var strBuffer = new StringBuilder();
            var buffer = new UnmanagedBuffer(generatedCode.ToArray());

            var disasm = new Disasm()
            {
                Archi = 64
            };

            int offset = 0;
            while (offset < generatedCode.Count)
            {
                disasm.EIP = new IntPtr(buffer.Ptr.ToInt64() + offset);
                int result = BeaEngine64.Disasm(disasm);

                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                {
                    break;
                }

                //strBuffer.AppendLine("0x" + offset.ToString("X") + " " + disasm.CompleteInstr);
                strBuffer.AppendLine(disasm.CompleteInstr);
                offset += result;
            }

            return strBuffer.ToString();
        }

        /// <summary>
        /// Disassembles the code block starting at the given index
        /// </summary>
        /// <param name="index">The start of the block</param>
        /// <param name="size">The size of the block</param>
        /// <param name="output">The output</param>
        public void DisassembleBlock(int index, int size, StringBuilder output)
        {
            int offset = index;
            while (offset < index + size)
            {
                this.disassembler.EIP = new IntPtr(this.codeBuffer.Ptr.ToInt64() + offset);
                int result = BeaEngine64.Disasm(this.disassembler);

                if (result == (int)BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                {
                    break;
                }

                output.AppendLine(this.disassembler.CompleteInstr);
                //output.AppendLine("0x" + this.disassembler.EIP.ToString("X") + " " + this.disassembler.CompleteInstr);
                offset += result;
            }
        }
    }
}
