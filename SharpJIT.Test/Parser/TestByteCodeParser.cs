using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Core;
using SharpJIT.Loader;

namespace SharpJIT.Test.Parser
{
    /// <summary>
    /// Tests the byte code parser
    /// </summary>
    [TestClass]
    public class TestByteCodeParser
    {
        /// <summary>
        /// Tests parsing a function
        /// </summary>
        [TestMethod]
        public void TestParseFunction()
        {
            var byteCodeParser = new ByteCodeParser(new Tokenizer().Tokenize("func std.println(Int) Void\n{\n}").ToList());
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);

            var function = assembly.Functions[0];
            Assert.AreEqual("std.println", function.Name);
            Assert.AreEqual(1, function.Parameters.Count);
            Assert.AreEqual("Int", function.Parameters[0]);
            Assert.AreEqual("Void", function.ReturnType);
        }

        /// <summary>
        /// Tests parsing a function
        /// </summary>
        [TestMethod]
        public void TestParseFunction2()
        {
            var byteCodeParser = new ByteCodeParser(new Tokenizer().Tokenize("func std.println(Int) Void\n{\nADDINT\n}").ToList());
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);

            var function = assembly.Functions[0];
            Assert.AreEqual(1, function.Instructions.Count);
            Assert.AreEqual(OpCodes.AddInt, function.Instructions[0].OpCode);
        }

        /// <summary>
        /// Tests parsing a function
        /// </summary>
        [TestMethod]
        public void TestParseFunction3()
        {
            var byteCodeParser = new ByteCodeParser(new Tokenizer().Tokenize("func std.println(Int) Void\n{\nNEWARR Int\n}").ToList());
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);

            var function = assembly.Functions[0];
            Assert.AreEqual(1, function.Instructions.Count);
            Assert.AreEqual(OpCodes.NewArray, function.Instructions[0].OpCode);
            Assert.AreEqual("Int", function.Instructions[0].StringValue);
        }

        /// <summary>
        /// Tokenizes the given file
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        private IList<Token> TokenizeFile(string fileName)
        {
            return new Tokenizer().Tokenize(File.ReadAllText($"../../Parser/Tests/{fileName}")).ToList();
        }

        /// <summary>
        /// Tests parsing int instructions
        /// </summary>
        [TestMethod]
        public void TestInt()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("int.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);
            var function = assembly.Functions[0];

            Assert.AreEqual(4, function.Instructions.Count);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[0].OpCode);
            Assert.AreEqual(0, function.Instructions[0].IntValue);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[1].OpCode);
            Assert.AreEqual(1, function.Instructions[1].IntValue);
            Assert.AreEqual(OpCodes.AddInt, function.Instructions[2].OpCode);
            Assert.AreEqual(OpCodes.Return, function.Instructions[3].OpCode);
        }

        /// <summary>
        /// Tests parsing locals
        /// </summary>
        [TestMethod]
        public void TestLocals()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("locals.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);
            var function = assembly.Functions[0];

            Assert.AreEqual(2, function.Locals.Count);
            Assert.AreEqual("Int", function.Locals[0]);
            Assert.AreEqual("Float", function.Locals[1]);
            Assert.AreEqual(1, function.Instructions.Count);
            Assert.AreEqual(OpCodes.Return, function.Instructions[0].OpCode);
        }

        /// <summary>
        /// Tests parsing a call
        /// </summary>
        [TestMethod]
        public void TestCall()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("call.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);
            var function = assembly.Functions[0];

            Assert.AreEqual(4, function.Instructions.Count);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[0].OpCode);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[1].OpCode);
            Assert.AreEqual(OpCodes.Call, function.Instructions[2].OpCode);
            Assert.AreEqual("addInternal", function.Instructions[2].StringValue);
            Assert.AreEqual(2, function.Instructions[2].Parameters.Count);
            Assert.AreEqual("Int", function.Instructions[2].Parameters[0]);
            Assert.AreEqual("Float", function.Instructions[2].Parameters[1]);
            Assert.AreEqual(OpCodes.Return, function.Instructions[3].OpCode);
        }

        /// <summary>
        /// Tests parsing a call instance
        /// </summary>
        [TestMethod]
        public void TestCallInstance()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("callinstance.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);
            var function = assembly.Functions[0];

            Assert.AreEqual(4, function.Instructions.Count);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[0].OpCode);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[1].OpCode);
            Assert.AreEqual(OpCodes.CallInstance, function.Instructions[2].OpCode);
            Assert.AreEqual("Point", function.Instructions[2].ClassType);
            Assert.AreEqual("add", function.Instructions[2].StringValue);
            Assert.AreEqual(2, function.Instructions[2].Parameters.Count);
            Assert.AreEqual("Int", function.Instructions[2].Parameters[0]);
            Assert.AreEqual("Float", function.Instructions[2].Parameters[1]);
            Assert.AreEqual(OpCodes.Return, function.Instructions[3].OpCode);
        }

        /// <summary>
        /// Tests parsing a class
        /// </summary>
        [TestMethod]
        public void TestClass()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("class.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Classes.Count);
            var currentClass = assembly.Classes[0];

            Assert.AreEqual("Point", currentClass.Name);
            Assert.AreEqual(2, currentClass.Fields.Count);
            Assert.AreEqual("x", currentClass.Fields[0].Name);
            Assert.AreEqual("Int", currentClass.Fields[0].Type);
            Assert.AreEqual("y", currentClass.Fields[1].Name);
            Assert.AreEqual("Int", currentClass.Fields[1].Type);
        }

        /// <summary>
        /// Tests parsing member function
        /// </summary>
        [TestMethod]
        public void TestMember()
        {
            var byteCodeParser = new ByteCodeParser(this.TokenizeFile("member.txt"));
            var assembly = byteCodeParser.Parse();

            Assert.AreEqual(1, assembly.Functions.Count);
            var function = assembly.Functions[0];
            Assert.AreEqual("add", function.Name);
            Assert.AreEqual("Point", function.ClassType);

            Assert.AreEqual(4, function.Instructions.Count);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[0].OpCode);
            Assert.AreEqual(0, function.Instructions[0].IntValue);
            Assert.AreEqual(OpCodes.LoadArgument, function.Instructions[1].OpCode);
            Assert.AreEqual(1, function.Instructions[1].IntValue);
            Assert.AreEqual(OpCodes.AddInt, function.Instructions[2].OpCode);
            Assert.AreEqual(OpCodes.Return, function.Instructions[3].OpCode);
        }
    }
}
