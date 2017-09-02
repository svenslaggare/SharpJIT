using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpJIT.Loader
{
    /// <summary>
    /// The token types
    /// </summary>
    public enum TokenType
    {
        Identifier,
        Int,
        Float,
        LeftRoundBracket,
        RightRoundBracket,
        LeftCurlyBracket,
        RightCurlyBracket,
        //Period,
        Colon,
        Function,
        Class,
        Member
    }

    /// <summary>
    /// Represents a token
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// The type of the token
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// The identifier
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The int value
        /// </summary>
        public int IntValue { get; }

        /// <summary>
        /// The float value
        /// </summary>
        public float FloatValue { get; }

        /// <summary>
        /// Creates a new token of the given type
        /// </summary>
        /// <param name="type">The type of the token</param>
        public Token(TokenType type)
        {
            this.Type = type;
            this.Identifier = "";
            this.IntValue = 0;
            this.FloatValue = 0.0f;
        }

        /// <summary>
        /// Creates a new string token
        /// </summary>
        /// <param name="value">The value</param>
        public Token(string value)
        {
            this.Type = TokenType.Identifier;
            this.Identifier = value;
            this.IntValue = 0;
            this.FloatValue = 0.0f;
        }

        /// <summary>
        /// Creates a new int token
        /// </summary>
        /// <param name="value">The value</param>
        public Token(int value)
        {
            this.Type = TokenType.Int;
            this.Identifier = "";
            this.IntValue = value;
            this.FloatValue = 0.0f;
        }

        /// <summary>
        /// Creates a new float token
        /// </summary>
        /// <param name="value">The value</param>
        public Token(float value)
        {
            this.Type = TokenType.Float;
            this.Identifier = "";
            this.IntValue = 0;
            this.FloatValue = value;
        }

        public override string ToString()
        {
            switch (this.Type)
            {
                case TokenType.Identifier:
                    return this.Identifier;
                case TokenType.Int:
                    return this.IntValue + "";
                case TokenType.Float:
                    return this.FloatValue + "";
                case TokenType.LeftRoundBracket:
                    return "(";
                case TokenType.RightRoundBracket:
                    return ")";
                case TokenType.LeftCurlyBracket:
                    return "{";
                case TokenType.RightCurlyBracket:
                    return "}";
                //case TokenType.Period:
                //    return ".";
                case TokenType.Colon:
                    return ":";
                case TokenType.Function:
                    return "func";
                case TokenType.Class:
                    return "class";
                case TokenType.Member:
                    return "member";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Represents a tokenizer
    /// </summary>
    public sealed class Tokenizer
    {
        private readonly IDictionary<char, TokenType> singleCharTokens = new Dictionary<char, TokenType>()
        {
            { '(', TokenType.LeftRoundBracket },
            { ')', TokenType.RightRoundBracket },
            { '{', TokenType.LeftCurlyBracket },
            { '}', TokenType.RightCurlyBracket },
            //{ '.', TokenType.Period },
            { ':', TokenType.Colon },
        };

        private readonly IDictionary<string, TokenType> identifierTokens = new Dictionary<string, TokenType>()
        {
            { "func", TokenType.Function },
            { "class", TokenType.Class },
            { "member", TokenType.Member },
        };

        /// <summary>
        /// Tokenizes the given text
        /// </summary>
        /// <param name="text">The text</param>
        public IEnumerable<Token> Tokenize(string text)
        {
            var i = 0;
            char CurrentChar()
            {
                return text[i];
            }

            while (i < text.Length)
            {
                if (char.IsWhiteSpace(CurrentChar()))
                {
                    i++;
                    continue;
                }
                else if (char.IsDigit(CurrentChar()))
                {
                    var numberString = CurrentChar().ToString();
                    while (true)
                    {
                        i++;
                        if (i >= text.Length)
                        {
                            break;
                        }

                        if (!char.IsDigit(CurrentChar()))
                        {
                            if (!char.IsWhiteSpace(CurrentChar()))
                            {
                                throw new ParserException($"Expected digit or whitespace.");
                            }

                            break;
                        }

                        numberString += CurrentChar();
                    }

                    yield return new Token(int.Parse(numberString));
                }
                else if (char.IsLetter(CurrentChar()) || CurrentChar() == '.')
                {
                    var identifier = CurrentChar().ToString();
                    var foundKeyword = false;

                    while (true)
                    {
                        i++;
                        if (i >= text.Length || !(char.IsLetterOrDigit(CurrentChar()) || CurrentChar() == '.' || CurrentChar() == ':'))
                        {
                            break;
                        }

                        identifier += CurrentChar();
                        if (this.identifierTokens.TryGetValue(identifier, out var tokenType))
                        {
                            i++;
                            foundKeyword = true;
                            yield return new Token(tokenType);
                            break;
                        }
                    }

                    if (!foundKeyword)
                    {
                        yield return new Token(identifier);
                    }
                }
                else if (singleCharTokens.TryGetValue(CurrentChar(), out var tokenType))
                {
                    i++;
                    yield return new Token(tokenType);
                }
                else
                {
                    i++;
                }
            }
        }
    }

}
