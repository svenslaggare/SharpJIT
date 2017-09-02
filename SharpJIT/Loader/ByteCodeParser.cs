using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpJIT.Loader.Data;

namespace SharpJIT.Loader
{
    /// <summary>
    /// Represents a parser exception
    /// </summary>
    public sealed class ParserException : Exception
    {
        /// <summary>
        /// Creates a new parser exception
        /// </summary>
        /// <param name="message">The message</param>
        public ParserException(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// Represents a byte code parser
    /// </summary>
    public sealed class ByteCodeParser
    {
        private readonly IList<Token> tokens;
        private int currentTokenIndex = 0;

        private readonly IDictionary<string, Core.OpCodes> noOperandsInstructions = new Dictionary<string, Core.OpCodes>()
        {
            //{ "nop", Core.OpCodes.Nop },
            { "pop", Core.OpCodes.Pop },
            //{ "dup", Core.OpCodes.Dup },
            { "addint", Core.OpCodes.AddInt },
            { "subint", Core.OpCodes.SubInt },
            { "mulint", Core.OpCodes.MulInt },
            { "divint", Core.OpCodes.DivInt },
            { "addfloat", Core.OpCodes.AddFloat },
            { "subfloat", Core.OpCodes.SubFloat },
            { "mulfloat", Core.OpCodes.MulFloat },
            { "divfloat", Core.OpCodes.DivFloat },
            { "ldtrue", Core.OpCodes.LoadTrue},
            { "ldfalse", Core.OpCodes.LoadFalse},
            { "and", Core.OpCodes.And },
            { "or", Core.OpCodes.Or },
            { "not", Core.OpCodes.Not },
            //{ "convinttofloat", Core.OpCodes.CONVERT_INT_TO_FLOAT },
            //{ "convfloattoint", Core.OpCodes.CONVERT_FLOAT_TO_INT },
            { "cmpeq", Core.OpCodes.CompareEqual },
            { "cmpne", Core.OpCodes.CompareNotEqual },
            { "cmpgt", Core.OpCodes.CompareGreaterThan },
            { "cmpge", Core.OpCodes.CompareGreaterThanOrEqual },
            { "cmplt", Core.OpCodes.CompareLessThan },
            { "cmple", Core.OpCodes.CompareLessThanOrEqual },
            { "ldnull", Core.OpCodes.LoadNull},
            { "ldlen", Core.OpCodes.LoadArrayLength },
            { "ret", Core.OpCodes.Return }
        };

        private readonly IDictionary<string, Core.OpCodes> branchInstructions = new Dictionary<string, Core.OpCodes>()
        {
            { "beq", Core.OpCodes.BranchEqual },
            { "bne", Core.OpCodes.BranchNotEqual },
            { "bgt", Core.OpCodes.BranchGreaterThan },
            { "bge", Core.OpCodes.BranchGreaterThanOrEqual },
            { "blt", Core.OpCodes.BranchLessThan},
            { "ble", Core.OpCodes.BranchLessThanOrEqual }
        };

        private readonly IDictionary<string, Core.OpCodes> stringOperandInstructions = new Dictionary<string, Core.OpCodes>()
        {
            { "newarr", Core.OpCodes.NewArray },
            { "stelem", Core.OpCodes.StoreElement },
            { "ldelem", Core.OpCodes.LoadElement },
            { "stfield", Core.OpCodes.StoreField },
            { "ldfield", Core.OpCodes.LoadField }
        };

        /// <summary>
        /// Creates a new byte code parser for the given tokens
        /// </summary>
        /// <param name="tokens">The tokens</param>
        public ByteCodeParser(IList<Token> tokens)
        {
            this.tokens = new List<Token>(tokens);
        }

        /// <summary>
        /// Returns the current token
        /// </summary>
        private Token CurrentToken => this.tokens[this.currentTokenIndex];

        /// <summary>
        /// Returns the current token as an identifier
        /// </summary>
        private string CurrentTokenAsIdentifier()
        {
            if (this.CurrentToken.Type != TokenType.Identifier)
            {
                throw new ParserException("Expected an identifier.");
            }

            return this.CurrentToken.Identifier;
        }

        /// <summary>
        /// Advances to the next token
        /// </summary>
        /// <returns>The next token</returns>
        private Token NextToken()
        {
            this.currentTokenIndex++;

            if (this.currentTokenIndex >= this.tokens.Count)
            {
                throw new ParserException("Reached end of tokens.");
            }

            return this.CurrentToken;
        }

        /// <summary>
        /// Advances to the next token at a token that can be at the end
        /// </summary>
        private void NextTokenAtEnd()
        {
            if (this.currentTokenIndex != this.tokens.Count - 1)
            {
                this.NextToken();
            }
            else
            {
                this.currentTokenIndex++;
            }
        }

        /// <summary>
        /// Advances to the next token and checks if the token is an identifier
        /// </summary>
        private string NextTokenAsIdentifier()
        {
            var token = this.NextToken();
            if (token.Type != TokenType.Identifier)
            {
                throw new ParserException("Expected an identifier.");
            }

            return token.Identifier;
        }

        /// <summary>
        /// Advances to the next token and checks if the token is an int
        /// </summary>
        private int NextTokenAsInt()
        {
            var value = this.NextToken();
            if (value.Type != TokenType.Int)
            {
                throw new ParserException("Expected an int.");
            }

            return value.IntValue;
        }

        /// <summary>
        /// Parses a function definition
        /// </summary>
        /// <param name="isExternal">Indicates if the function is external</param>
        private (string, IList<string>, string) ParseFunctionDefinition(bool isExternal = false)
        {
            var name = this.NextTokenAsIdentifier();

            if (this.NextToken().Type != TokenType.LeftRoundBracket)
            {
                throw new ParserException("Expected '(' after function name..");
            }

            var parameters = new List<string>();
            while (true)
            {
                var parameter = this.NextToken();
                if (parameter.Type == TokenType.RightRoundBracket)
                {
                    break;
                }

                if (parameter.Type != TokenType.Identifier)
                {
                    throw new ParserException("Expected an identifier.");
                }

                parameters.Add(parameter.Identifier);
            }

            var returnType = this.NextTokenAsIdentifier();
            return (name, parameters, returnType);
        }

        /// <summary>
        /// Parses a function body
        /// </summary>
        private (IList<string>, IList<Instruction>) ParseFunctionBody()
        {
            var locals = new List<string>();
            var instructions = new List<Instruction>();

            this.NextToken();
            if (this.CurrentToken.Type != TokenType.LeftCurlyBracket)
            {
                throw new ParserException("Expected '{' after function definition.");
            }
            this.NextToken();

            var localsSet = false;
            while (true)
            {
                if (this.CurrentToken.Type == TokenType.RightCurlyBracket)
                {
                    this.NextTokenAtEnd();
                    break;
                }
                else
                {
                    this.ParseInstruction(locals, instructions, ref localsSet);
                }

                this.NextToken();
            }

            return (locals, instructions);
        }

        /// <summary>
        /// Reads parameters used for calls
        /// </summary>
        private IList<string> ReadCallParameters()
        {
            var parameters = new List<string>();

            while (true)
            {
                var parameter = this.NextToken();
                if (parameter.Type == TokenType.RightRoundBracket)
                {
                    break;
                }

                parameters.Add(this.CurrentTokenAsIdentifier());
            }

            return parameters;
        }

        /// <summary>
        /// Parses an instruction
        /// </summary>
        /// <param name="locals">The locals</param>
        /// <param name="instructions">The instructions</param>
        /// <param name="localsSet">Indicates if the locals are set</param>
        private void ParseInstruction(IList<string> locals, IList<Instruction> instructions, ref bool localsSet)
        {
            var currentTokenAsLower = this.CurrentTokenAsIdentifier().ToLower();

            switch (currentTokenAsLower)
            {
                case "ldint":
                    {
                        var value = this.NextTokenAsInt();
                        instructions.Add(new Instruction(Core.OpCodes.LoadInt, value));
                        return;
                    }
                case "ldfloat":
                    {
                        var value = this.NextToken();
                        if (value.Type != TokenType.Float)
                        {
                            throw new ParserException("Expected a float.");
                        }

                        instructions.Add(new Instruction(Core.OpCodes.LoadFloat, value.FloatValue));
                        return;
                    }
                case string _ when this.noOperandsInstructions.ContainsKey(currentTokenAsLower):
                    {
                        instructions.Add(new Instruction(this.noOperandsInstructions[currentTokenAsLower]));
                        return;
                    }
                case string _ when this.stringOperandInstructions.ContainsKey(currentTokenAsLower):
                    {
                        var value = this.NextTokenAsIdentifier();
                        instructions.Add(new Instruction(this.stringOperandInstructions[currentTokenAsLower], value));
                        return;
                    }
                case ".locals":
                    {
                        if (!localsSet)
                        {
                            var numLocals = this.NextTokenAsInt();

                            if (numLocals >= 0)
                            {
                                localsSet = true;
                                for (int i = 0; i < numLocals; i++)
                                {
                                    locals.Add("");
                                }

                                return;
                            }
                            else
                            {
                                throw new ParserException("The number of locals must be >= 0.");
                            }
                        }
                        else
                        {
                            throw new ParserException("The locals has already been set.");
                        }
                    }
                case ".local":
                    {
                        if (localsSet)
                        {
                            var localIndex = this.NextTokenAsInt();
                            var localType = this.NextTokenAsIdentifier();

                            if (localIndex >= 0 && localIndex < locals.Count)
                            {
                                locals[localIndex] = localType;
                                return;
                            }
                            else
                            {
                                throw new ParserException("Invalid local index.");
                            }
                        }
                        else
                        {
                            throw new ParserException("The locals must been set.");
                        }
                    }
                case "ldloc":
                case "stloc":
                    {
                        if (!localsSet)
                        {
                            throw new ParserException("The locals must be set.");
                        }

                        var localIndex = this.NextTokenAsInt();
                        if (localIndex >= 0 && localIndex < locals.Count)
                        {
                            instructions.Add(new Instruction(
                                currentTokenAsLower == "ldloc" ? Core.OpCodes.LoadLocal : Core.OpCodes.StoreLocal,
                                localIndex));
                            return;
                        }
                        else
                        {
                            throw new ParserException("The local index is out of range.");
                        }
                    }
                case "call":
                case "callinst":
                    {
                        bool isInstance = currentTokenAsLower == "callinst";

                        var toCallName = this.NextTokenAsIdentifier();
                        var classType = "";

                        if (isInstance)
                        {
                            var classNamePosition = toCallName.IndexOf("::");
                            if (classNamePosition != -1)
                            {
                                classType = toCallName.Substring(0, classNamePosition);
                            }
                            else
                            {
                                throw new ParserException("Expected '::' in called member function.");
                            }

                            toCallName = toCallName.Substring(classNamePosition + 2);
                        }

                        if (this.NextToken().Type != TokenType.LeftRoundBracket)
                        {
                            throw new ParserException("Expected '(' after called function.");
                        }

                        var parameters = this.ReadCallParameters();
                        if (isInstance)
                        {
                            instructions.Add(new Instruction(Core.OpCodes.CallInstance, toCallName, classType, parameters));
                        }
                        else
                        {
                            instructions.Add(new Instruction(Core.OpCodes.Call, toCallName, parameters));
                        }

                        return;
                    }
                case "ldarg":
                    {
                        var argumentIndex = this.NextTokenAsInt();
                        instructions.Add(new Instruction(Core.OpCodes.LoadArgument, argumentIndex));
                        return;
                    }
                case "newobj":
                    {
                        var toCallName = this.NextTokenAsIdentifier();
                        var classType = "";

                        var classNamePosition = toCallName.IndexOf("::");
                        if (classNamePosition != -1)
                        {
                            classType = toCallName.Substring(0, classNamePosition);
                        } else
                        {
                            throw new ParserException("Expected '::' after the type in a new object instruction.");
                        }

                        toCallName = toCallName.Substring(classNamePosition + 2);

                        if (toCallName != ".constructor")
                        {
                            throw new ParserException("Expected call to constructor.");
                        }

                        if (this.NextToken().Type != TokenType.LeftRoundBracket)
                        {
                            throw new ParserException("Expected '(' after called function.");
                        }

                        var parameters = this.ReadCallParameters();
                        instructions.Add(new Instruction(Core.OpCodes.NewObject, toCallName, classType, parameters));
                        return;
                    }
                case "br":
                    {
                        var target = this.NextTokenAsInt();
                        instructions.Add(new Instruction(Core.OpCodes.Branch, target));
                        return;
                    }
                case string _ when this.branchInstructions.ContainsKey(currentTokenAsLower):
                    {
                        var target = this.NextTokenAsInt();
                        instructions.Add(new Instruction(this.branchInstructions[currentTokenAsLower], target));
                        return;
                    }
            }

            throw new ParserException($"'{currentTokenAsLower}' is not a valid instruction.");
        }

        /// <summary>
        /// Parses a class body
        /// </summary>
        private IList<Field> ParseClassBody()
        {
            this.NextToken();
            if (this.CurrentToken.Type != TokenType.LeftCurlyBracket)
            {
                throw new ParserException("Expected '{' after function definition.");
            }
            this.NextToken();

            var fields = new List<Field>();

            while (true)
            {
                if (this.CurrentToken.Type == TokenType.RightCurlyBracket)
                {
                    this.NextTokenAtEnd();
                    break;
                }
                else
                {
                    var fieldName = this.CurrentTokenAsIdentifier();
                    var fieldType = this.NextTokenAsIdentifier();
                    fields.Add(new Field(fieldName, fieldType, Core.Objects.AccessModifier.Public));
                }

                this.NextToken();
            }

            return fields;
        }

        /// <summary>
        /// Parses the loaded tokens
        /// </summary>
        /// <returns>The parsed assembly</returns>
        /// <param name="name">The name of the assembly</param>
        public Assembly Parse(string name = "")
        {
            var classes = new List<Class>();
            var functions = new List<Function>();

            while (this.currentTokenIndex < this.tokens.Count)
            {
                var topLevelToken = this.CurrentToken;
                switch (topLevelToken.Type)
                {
                    case TokenType.Function:
                        {
                            (var functionName, var parameters, var returnType) = this.ParseFunctionDefinition(false);
                            if (functionName.Contains("::"))
                            {
                                throw new ParserException("'::' is only allowed in member functions.");
                            }

                            (var locals, var instructions) = this.ParseFunctionBody();
                            functions.Add(new Function(functionName, parameters, returnType, locals, instructions));
                            break;
                        }
                    case TokenType.Class:
                        {
                            var className = this.NextTokenAsIdentifier();
                            var fields = this.ParseClassBody();
                            classes.Add(new Class(className, fields));
                            break;
                        }
                    case TokenType.Member:
                        {
                            (var functionName, var parameters, var returnType) = this.ParseFunctionDefinition(false);
                            if (!functionName.Contains("::"))
                            {
                                throw new ParserException("Expected '::' in member function name.");
                            }

                            var classNamePosition = functionName.IndexOf("::");
                            var classTypeName = functionName.Substring(0, classNamePosition);
                            var memberFunctionName = functionName.Substring(classNamePosition + 2);

                            (var locals, var instructions) = this.ParseFunctionBody();
                            functions.Add(new Function(
                                memberFunctionName,
                                parameters,
                                returnType,
                                classTypeName,
                                memberFunctionName == ".constructor",
                                locals,
                                instructions));
                            break;
                        }
                    default:
                        break;
                }
            }

            return new Assembly(name, classes, functions);
        }

        /// <summary>
        /// Parses the given text as an assembly
        /// </summary>
        /// <param name="name">The name of the assembly</param>
        /// <param name="text">The content of the assembly</param>
        public static Assembly Parse(string name, string text)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(text).ToList();
            var byteCodeParser = new ByteCodeParser(tokens);
            return byteCodeParser.Parse(name);
        }
    }
}
