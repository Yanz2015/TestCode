using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMediaIntelligence.Business.Models.Query
{

    public enum Operator
    {
        Equal,
        NotEqual,
        Greater,
        NoLess,
        Less,
        NoMore
    }

    /// <summary>
    /// The token reader to read the token from the expression.
    /// </summary>
    public class TokenReader
    {
        public TokenReader(string sentence)
        {
            this.sentence = sentence;
        }

        /// <summary>
        /// The sentence to be tokened.
        /// </summary>
        private string sentence = string.Empty;

        /// <summary>
        /// current read index.
        /// </summary>
        private int currentIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        private Token currentToken = null;

        /// <summary>
        /// Read next token from the sentence.
        /// </summary>
        /// <returns></returns>
        public Token ReadNextToken()
        {
            if (this.sentence.Length > this.currentIndex)
            {
                this.SkipWhiteSpace();

                var text = this.ReadNextTokenStr();
                
                if (this.IsOperatorText(text))
                {
                    this.currentToken = new Token { Text = text, Type = TokenType.Operator };
                }
                else
                {
                    this.currentToken = new Token { Text = text, Type = TokenType.Expression };
                }
            }
            else
            {
                return null;
            }

            return this.currentToken;
        }

        public int GetTokenPriority(Token token)
        {
            var dict = this.GetOperatorPriorities();
            if (dict.ContainsKey(token.Text))
            {
                return dict[token.Text];
            }
            else
            {
                throw new ArgumentException($"The given token text {token.Text} is not expected", nameof(token));
            }
        }

        /// <summary>
        /// Skip White Space while Reading Token.
        /// </summary>
        private void SkipWhiteSpace()
        {
            char c = this.sentence.ElementAt(this.currentIndex);
            while (char.IsWhiteSpace(c))
            {
                if (this.sentence.Length > this.currentIndex + 1)
                {
                    this.currentIndex++;
                    c = this.sentence.ElementAt(this.currentIndex);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Read next token string.
        /// </summary>
        /// <returns>The token string.</returns>
        private string ReadNextTokenStr()
        {
            int startIndex = this.currentIndex;
            if (this.sentence.Length > this.currentIndex)
            {
                char c = this.sentence.ElementAt(this.currentIndex);
                if (!char.IsLetterOrDigit(c) && c != '\'' && c != '"')
                {
                    this.currentIndex++;
                }
                else
                {
                    if (c == '\'' || c == '"')
                    {
                        char nc = '\0';
                        while (nc != c && this.sentence.Length > this.currentIndex)
                        {
                            this.currentIndex++;
                            if (this.currentIndex < this.sentence.Length)
                            {
                                nc = this.sentence.ElementAt(this.currentIndex);
                            }
                            else
                            {
                                break;
                            }
                        }

                        this.currentIndex++;
                    }
                    else
                    {
                        while (char.IsLetterOrDigit(c) || c == '-')
                        {
                            if (this.sentence.Length > this.currentIndex)
                            {
                                this.currentIndex++;
                                if (this.currentIndex < this.sentence.Length)
                                {
                                    c = this.sentence.ElementAt(this.currentIndex);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                var tokenStr = this.sentence.Substring(startIndex, this.currentIndex - startIndex);
                this.CheckTokenTextAvailable(tokenStr);
                if (tokenStr.Contains('\'') || tokenStr.Contains('"'))
                {
                    var start = tokenStr.ElementAt(0);
                    var end = tokenStr.ElementAt(tokenStr.Length - 1);
                    if (start != end || (start != '\'' && start != '"'))
                    {
                        throw new NotSupportedException($"Error Format near the token {startIndex}");
                    }

                    tokenStr = tokenStr.Trim('\'', '"');
                    if (tokenStr.Contains('\'') || tokenStr.Contains('"'))
                    {
                        throw new NotSupportedException($"Error Format near the token {startIndex}");
                    }
                }

                return tokenStr;
            }
            return null;
        }

        /// <summary>
        /// Whether the given character is a spliter.
        /// </summary>
        /// <param name="c">the given character.</param>
        /// <returns>Whether the given character is a spliter.</returns>
        private bool IsSpliter(char c)
        {
            HashSet<char> spliters = new HashSet<char> { '(', ')', '\t', '\n', ' ' };
            return spliters.Contains(c);
        }

        private bool IsQuota(char c)
        {
            HashSet<char> spliters = new HashSet<char> { '\'', '"' };
            return spliters.Contains(c);
        }


        private bool IsOperatorText(string text)
        {
            HashSet<string> operators = new HashSet<string>
                                            {
                                                "eq",
                                                "ne",
                                                "gt",
                                                "ge",
                                                "lt",
                                                "le",
                                                "and",
                                                "or",
                                                "not",
                                                "(",
                                                ")"
                                            };
            return operators.Contains(text);
        }

        private void CheckTokenTextAvailable(string text)
        {
            int index = 0;

            if (text.StartsWith("'") && text.EndsWith("'") || text.StartsWith("\"") && text.EndsWith("\"") || text=="(" || text==")")
            {
                return;
            }

            while (index < text.Length)
            {
                char c = text.ElementAt(index);
                if (!char.IsLetterOrDigit(c) && c != '-')
                {
                    throw new ArgumentException($"Non Expected name or value filed {text}");
                }
                index++;
            }
        }

        private IDictionary<string, int> GetOperatorPriorities()
        {
            IDictionary<string, int> priorities = new Dictionary<string, int>()
                                                      {
                                                          { "eq", 3 },
                                                          { "ne", 3 },
                                                          { "gt", 3 },
                                                          { "ge", 3 },
                                                          { "lt", 3 },
                                                          { "le", 3 },
                                                          { "and", 2 },
                                                          { "or", 1 },
                                                          { "(", 4 },
                                                          { ")" ,0 },

                                                      };
            return priorities;
        }
    }
}
