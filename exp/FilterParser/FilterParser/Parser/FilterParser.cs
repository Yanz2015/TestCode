namespace MCMediaIntelligence.Business.Models.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using MCMediaIntelligence.Business.Models.Query;

    public class TwoFactorExpression
    {
        public Token OperatorToken { get; set; }

        public TwoFactorExpression LeftExpression { get; set; }

        public TwoFactorExpression RightExpression { get; set; }

        public Token ValueToken { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.ValueToken != null)
            {
                sb.Append(this.ValueToken.Text);
                if (this.LeftExpression != null || this.RightExpression != null)
                {
                    throw new Exception("Unexpected Error");
                }
                return sb.ToString();
            }

            if (this.LeftExpression?.ValueToken == null)
            {
                sb.Append("(");
                sb.Append(this.LeftExpression);
                sb.Append(")");
            }
            else if (this.LeftExpression?.ValueToken != null)
            {
                sb.Append(this.LeftExpression);
            }
            sb.Append(this.OperatorToken.Text);
            if (this.RightExpression?.ValueToken == null)
            {
                sb.Append("(");
                sb.Append(this.RightExpression);
                sb.Append(")");
            }
            else if (this.RightExpression?.ValueToken != null)
            {
                sb.Append(this.RightExpression);
            }

            return sb.ToString();
        }
    }

    public class FilterParser
    {

        /// <summary>
        /// The stack for operator.
        /// </summary>
        private readonly Stack<Token> operatorStack = new Stack<Token>();

        /// <summary>
        /// The stack for data.
        /// </summary>
        private readonly Stack<TwoFactorExpression> dataStack = new Stack<TwoFactorExpression>();

        /// <summary>
        /// The token reader used to 
        /// </summary>
        private TokenReader reader;

        private int expectedBracket = 0;

        /// <summary>
        /// Parse the filter string in to filter model collection.
        /// </summary>
        /// <param name="filterStr">The filter string to be parsed.</param>
        /// <returns>The filter Model collection.</returns>
        public FilterModelCollection ParseFilter(string filterStr)
        {
            this.reader = new TokenReader(filterStr);
            Token token;
            do
            {
                token = this.reader.ReadNextToken();
                if (token != null)
                {
                    if (token.Type == TokenType.Operator)
                    {
                        if (token.Text == ")")
                        {
                            this.expectedBracket --;
                        }

                        if (this.operatorStack.Count > 0)
                        {
                            Token lastToken = this.operatorStack.Peek();
                            if (this.HighPriorityToken(lastToken, token))
                            {
                                this.PushOperator(token);
                            }
                            else
                            {
                                while (!this.HighPriorityToken(lastToken, token) && this.operatorStack.Count != 0)
                                {
                                    this.CalculateOprator();
                                    if (this.operatorStack.Count > 0)
                                    {
                                        lastToken = this.operatorStack.Peek();
                                    }
                                }

                                if (token.Text != ")")
                                {
                                    this.PushOperator(token);
                                }
                                else
                                {
                                    var matchToken = this.operatorStack.Peek();
                                    if (matchToken.Text != "(")
                                    {
                                        throw new ArgumentException("Unmatched bracket");
                                    }
                                    else
                                    {
                                        this.operatorStack.Pop();
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (token.Text != ")")
                            {
                                this.PushOperator(token);
                            }
                            else
                            {
                                throw new ArgumentException("Unmatched bracket");
                            }
                        }
                    }
                    else
                    {
                        TwoFactorExpression exp = new TwoFactorExpression { ValueToken = token };
                        this.dataStack.Push(exp);
                    }
                }
            }
            while (token != null);


            while (this.operatorStack.Count > 0)
            {
                this.CalculateOprator();
            }

            if (this.expectedBracket != 0)
            {
                throw new ArgumentException("unmatched bracket");
            }

            if (this.dataStack.Count > 1)
            {
                throw new ArgumentException("The parse parse a wrong result based on given filter");
            }
            else
            {
                return this.BuildFilterFromExpression(this.dataStack.Pop());
            }

        }



        private FilterModelCollection BuildFilterFromExpression(TwoFactorExpression expression)
        {
            if (expression == null)
            {
                return null;
            }

            Debug.WriteLine(expression.ToString());
            FilterModelCollection collection = new FilterModelCollection();
            if (expression.OperatorToken.Text == "and")
            {
                collection.Operator = LogicOperator.And;
                if (expression.LeftExpression != null)
                {
                    var child = this.BuildFilterFromExpression(expression.LeftExpression);
                    collection.Filters.Add(child);
                }

                if (expression.RightExpression != null)
                {
                    var child = this.BuildFilterFromExpression(expression.RightExpression);
                    collection.Filters.Add(child);
                }
            }
            else if (expression.OperatorToken.Text == "or")
            {
                collection.Operator = LogicOperator.Or;
                if (expression.LeftExpression != null)
                {
                    var child = this.BuildFilterFromExpression(expression.LeftExpression);
                    collection.Filters.Add(child);
                }

                if (expression.RightExpression != null)
                {
                    var child = this.BuildFilterFromExpression(expression.RightExpression);
                    collection.Filters.Add(child);
                }
            }
            else
            {
                if (expression.LeftExpression.LeftExpression != null
                    || expression.LeftExpression.RightExpression != null
                    || expression.RightExpression.LeftExpression != null
                    || expression.RightExpression.RightExpression != null)
                {
                    throw new ArgumentException(
                        "Non logic expression should not have sub expression",
                        nameof(expression));
                }
                FilterModel model = new FilterModel();
                model.Name = expression.LeftExpression.ValueToken.Text;
                model.Value = expression.RightExpression.ValueToken.Text;
                model.Operator = expression.OperatorToken.Text;
                collection.Filters.Add(model);
            }

            return collection;

        }

        /// <summary>
        /// Calculator operator and get the result TwoFactorExpression.
        /// </summary>
        private void CalculateOprator()
        {
            var op = this.operatorStack.Pop();
            if (op.Text != "(")
            {
                try
                {

                    var right = this.dataStack.Pop();
                    var left = this.dataStack.Pop();

                    TwoFactorExpression exp = new TwoFactorExpression()
                                                  {
                                                      RightExpression = right,
                                                      LeftExpression = left,
                                                      OperatorToken = op
                                                  };

                    this.dataStack.Push(exp);
                }
                catch
                {
                    throw new ArgumentException($"Wrong Expression Passed Near {op.Text}");
                }
            }
        }


        /// <summary>
        /// Judge if the token priority is higher than the last one.
        /// </summary>
        /// <param name="lastToken">The token to be compared.</param>
        /// <param name="nextToken">The current token.</param>
        /// <returns>whether the current token priority is higher.</returns>
        private bool HighPriorityToken(Token lastToken, Token nextToken)
        {
            if (nextToken == null)
            {
                throw new ArgumentNullException("nextToken");
            }

            if (lastToken == null)
            {
                return true;
            }

            if (nextToken.Text == "(")
            {
                return true;
            }
            else if (lastToken.Text == "(")
            {
                return true;
            }

            var first = this.reader.GetTokenPriority(lastToken);
            var next = this.reader.GetTokenPriority(nextToken);
            return next >= first;
        }

        private void PushOperator(Token oper)
        {
            if (oper.Text == "(")
            {
                this.expectedBracket++;
            }
            this.operatorStack.Push(oper);
        }
    }
}
