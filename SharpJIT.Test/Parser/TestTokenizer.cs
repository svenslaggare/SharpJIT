using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpJIT.Loader;

namespace SharpJIT.Test.Parser
{
    /// <summary>
    /// Tests the <see cref="SharpJIT.Loader.Tokenizer"/>
    /// </summary>
    [TestClass]
    public class TestTokenizer
    {
        /// <summary>
        /// Tests parsing single character tokens
        /// </summary>
        [TestMethod]
        public void TestParseTokens()
        {
            var tokenizer = new Tokenizer();
            List<Token> tokens = null;

            tokens = tokenizer.Tokenize("(").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.LeftRoundBracket, tokens[0].Type);

            tokens = tokenizer.Tokenize(")").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.RightRoundBracket, tokens[0].Type);

            tokens = tokenizer.Tokenize("{").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.LeftCurlyBracket, tokens[0].Type);

            tokens = tokenizer.Tokenize("}").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.RightCurlyBracket, tokens[0].Type);

            //tokens = tokenizer.Tokenize(".").ToList();
            //Assert.AreEqual(1, tokens.Count);
            //Assert.AreEqual(TokenType.Period, tokens[0].Type);

            tokens = tokenizer.Tokenize(":").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Colon, tokens[0].Type);
        }

        /// <summary>
        /// Tests parsing identifier tokens
        /// </summary>
        [TestMethod]
        public void TestParseTokens2()
        {
            var tokenizer = new Tokenizer();
            List<Token> tokens = null;

            tokens = tokenizer.Tokenize("func").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Function, tokens[0].Type);

            tokens = tokenizer.Tokenize("class").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Class, tokens[0].Type);

            tokens = tokenizer.Tokenize("member").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Member, tokens[0].Type);
        }

        /// <summary>
        /// Tests parsing an int
        /// </summary>
        [TestMethod]
        public void TestParseInt()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("1342571065").ToList();

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Int, tokens[0].Type);
            Assert.AreEqual(1342571065, tokens[0].IntValue);
        }

        /// <summary>
        /// Tests parsing an int
        /// </summary>
        [TestMethod]
        public void TestParseInt2()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("0").ToList();

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Int, tokens[0].Type);
            Assert.AreEqual(0, tokens[0].IntValue);
        }

        /// <summary>
        /// Tests parsing an int
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ParserException))]
        public void TestParseInt3()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("1342571065aa").ToList();
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Int, tokens[0].Type);
            Assert.AreEqual(1342571065, tokens[0].IntValue);
        }

        /// <summary>
        /// Tests parsing an identifier
        /// </summary>
        [TestMethod]
        public void TestParseIdentifier()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("df244350gg34").ToList();

            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
            Assert.AreEqual("df244350gg34", tokens[0].Identifier);
        }

        /// <summary>
        /// Tests parsing an identifier
        /// </summary>
        [TestMethod]
        public void TestParseIdentifier2()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("df244350gg34 grfge21r425").ToList();

            Assert.AreEqual(2, tokens.Count);
            Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
            Assert.AreEqual("df244350gg34", tokens[0].Identifier);

            Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
            Assert.AreEqual("grfge21r425", tokens[1].Identifier);
        }

        /// <summary>
        /// Tests parsing multiple tokens
        /// </summary>
        [TestMethod]
        public void TestParseMultiple()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("31431 df244350gg34 . grfge21r425{}(.)").ToList();

            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(31431, tokens[0].IntValue);
            Assert.AreEqual("df244350gg34", tokens[1].Identifier);
            Assert.AreEqual(".", tokens[2].Identifier);
            Assert.AreEqual("grfge21r425", tokens[3].Identifier);
            Assert.AreEqual(TokenType.LeftCurlyBracket, tokens[4].Type);
            Assert.AreEqual(TokenType.RightCurlyBracket, tokens[5].Type);
            Assert.AreEqual(TokenType.LeftRoundBracket, tokens[6].Type);
            Assert.AreEqual(".", tokens[7].Identifier);
            Assert.AreEqual(TokenType.RightRoundBracket, tokens[8].Type);
        }

        /// <summary>
        /// Tests parsing multiple tokens
        /// </summary>
        [TestMethod]
        public void TestParseMultiple2()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("func std.println(Int) Void\n{\tLDARG 0\n}").ToList();

            Assert.AreEqual(10, tokens.Count);
            Assert.AreEqual(TokenType.Function, tokens[0].Type);
            Assert.AreEqual("std.println", tokens[1].Identifier);
            Assert.AreEqual(TokenType.LeftRoundBracket, tokens[2].Type);
            Assert.AreEqual("Int", tokens[3].Identifier);
            Assert.AreEqual(TokenType.RightRoundBracket, tokens[4].Type);
            Assert.AreEqual("Void", tokens[5].Identifier);
            Assert.AreEqual(TokenType.LeftCurlyBracket, tokens[6].Type);
            Assert.AreEqual("LDARG", tokens[7].Identifier);
            Assert.AreEqual(0, tokens[8].IntValue);
            Assert.AreEqual(TokenType.RightCurlyBracket, tokens[9].Type);
        }

        /// <summary>
        /// Tests parsing multiple tokens
        /// </summary>
        [TestMethod]
        public void TestParseMultiple3()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("func std.println(Int) Void\n{\tLDARG 0\n}\n\nfunc add(Int Int) Int\n{\nADD\n}").ToList();

            Assert.AreEqual(20, tokens.Count);
            Assert.AreEqual(TokenType.Function, tokens[0].Type);
            Assert.AreEqual("std.println", tokens[1].Identifier);
            Assert.AreEqual(TokenType.LeftRoundBracket, tokens[2].Type);
            Assert.AreEqual("Int", tokens[3].Identifier);
            Assert.AreEqual(TokenType.RightRoundBracket, tokens[4].Type);
            Assert.AreEqual("Void", tokens[5].Identifier);
            Assert.AreEqual(TokenType.LeftCurlyBracket, tokens[6].Type);
            Assert.AreEqual("LDARG", tokens[7].Identifier);
            Assert.AreEqual(0, tokens[8].IntValue);
            Assert.AreEqual(TokenType.RightCurlyBracket, tokens[9].Type);

            Assert.AreEqual(TokenType.Function, tokens[10].Type);
            Assert.AreEqual("add", tokens[11].Identifier);
            Assert.AreEqual(TokenType.LeftRoundBracket, tokens[12].Type);
            Assert.AreEqual("Int", tokens[13].Identifier);
            Assert.AreEqual("Int", tokens[14].Identifier);
            Assert.AreEqual(TokenType.RightRoundBracket, tokens[15].Type);
            Assert.AreEqual("Int", tokens[16].Identifier);
            Assert.AreEqual(TokenType.LeftCurlyBracket, tokens[17].Type);
            Assert.AreEqual("ADD", tokens[18].Identifier);
            Assert.AreEqual(TokenType.RightCurlyBracket, tokens[19].Type);
        }
    }
}
