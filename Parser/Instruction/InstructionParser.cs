namespace CorpseLib.Scripts.Parser.Instruction
{
    public partial class InstructionParser
    {
        private static int[] ConvertNameFromToken(ExpressionToken token, ParsingContext parsingContext)
        {
            List<int> ids = [];
            string[] nameParts = token.Token.Split('.');
            foreach (string part in nameParts)
                ids.Add(parsingContext.PushName(part));
            return ids.ToArray();
        }

        public static OperationResult<AExpression> Parse(string input, ParsingContext parsingContext)
        {
            if (string.IsNullOrEmpty(input))
                return new("Error while parsing instruction", "Input is empty");
            TokenReader tokens = new(input);
            ExpressionToken currentToken = tokens.Current!;
            if (currentToken.IsUnaryMutation && (tokens[1]?.IsIdentifier ?? false))
            {
                string op = currentToken.Token;
                tokens.Pop();
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                return new(new UnaryMutationExpression(new VariableExpression(nameIDs), op, true));
            }
            else if (currentToken.IsIdentifier && (tokens[1]?.IsIdentifier ?? false) && tokens[2]?.Token == "=")
            {
                string type = currentToken.Token;
                tokens.Pop();
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                tokens.Pop();
                OperationResult<AExpression> exprResult = ParseExpression(tokens, parsingContext);
                if (!exprResult)
                    return exprResult;
                AExpression expr = exprResult.Result!;
                return new(new AssignmentExpression(type, nameIDs, expr));
            }
            else if (currentToken.IsIdentifier && (tokens[1]?.IsCompoundOperator ?? false))
            {
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                string op = tokens.Current!.Token[..^1];
                tokens.Pop();
                OperationResult<AExpression> exprResult = ParseExpression(tokens, parsingContext);
                if (!exprResult)
                    return exprResult;
                AExpression expr = exprResult.Result!;
                return new(new CompoundAssignmentExpression(nameIDs, op, expr));
            }
            else if (currentToken.IsIdentifier && tokens[1]?.Token == "=")
            {
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                tokens.Pop();
                OperationResult<AExpression> exprResult = ParseExpression(tokens, parsingContext);
                if (!exprResult)
                    return exprResult;
                AExpression expr = exprResult.Result!;
                return new(new AssignmentExpression(null, nameIDs, expr));
            }
            else if (currentToken.IsIdentifier && (tokens[1]?.IsUnaryMutation ?? false))
            {
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                string op = tokens.Current!.Token;
                tokens.Pop();
                return new(new UnaryMutationExpression(new VariableExpression(nameIDs), op, false));
            }
            return ParseExpression(tokens, parsingContext);
        }

        private static OperationResult<AExpression> ParseExpression(TokenReader tokens, ParsingContext parsingContext, int precedence = 0)
        {
            OperationResult<AExpression> leftResult = ParsePrimary(tokens, parsingContext);
            if (!leftResult)
                return leftResult;
            AExpression left = leftResult.Result!;
            while (tokens.HasNext)
            {
                string op = tokens.Current!.Token;
                int prec = op switch
                {
                    "=" => 0,
                    "||" => 1,
                    "&&" => 2,
                    "==" or "!=" => 3,
                    "<" or ">" or "<=" or ">=" => 4,
                    "+" or "-" => 5,
                    "*" or "/" or "%" => 6,
                    _ => -1
                };
                if (prec < precedence || prec < 0)
                    break;
                tokens.Pop();
                int nextPrecedence = prec + (op == "=" ? 0 : 1);
                OperationResult<AExpression> rightResult = ParseExpression(tokens, parsingContext, nextPrecedence);
                if (!rightResult)
                    return rightResult;
                AExpression right = rightResult.Result!;
                if (op == "=")
                {
                    if (left is VariableExpression varExpr)
                        left = new AssignmentExpression(null, varExpr.IDs, right);
                    else
                        return new("Error while parsing instruction", "Left side of '=' must be a variable.");
                }
                else
                    left = new BinaryExpression(op, left, right);
            }
            return new(left);
        }

        private static OperationResult<AExpression> ParsePrimary(TokenReader tokens, ParsingContext parsingContext)
        {
            ExpressionToken? currentToken = tokens.Current;
            if (currentToken == null)
                return new("Error while parsing instruction", "tokens are empty");
            if (currentToken.IsUnaryMutation)
            {
                string op = currentToken.Token;
                tokens.Pop();
                OperationResult<AExpression> target = ParsePrimary(tokens, parsingContext);
                if (!target)
                    return target;
                return new(new UnaryMutationExpression(target.Result!, op, true));
            }
            else if (currentToken.IsUnaryOperator)
            {
                string op = currentToken.Token;
                tokens.Pop();
                OperationResult<AExpression> operand = ParsePrimary(tokens, parsingContext);
                if (!operand)
                    return operand;
                return new(new UnaryExpression(op, operand.Result!));
            }
            else if (currentToken.Token == "(")
            {
                tokens.Pop();
                OperationResult<AExpression> expr = ParseExpression(tokens, parsingContext);
                if (tokens.Current?.Token != ")")
                    throw new Exception($"Expected ')' but found '{tokens.Current}'");
                tokens.Pop();
                return expr;
            }
            else if (currentToken.IsLiteral)
            {
                string literal = currentToken.Token;
                tokens.Pop();
                return new(new LiteralExpression(literal));
            }
            else if (currentToken.IsIdentifier)
            {
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                if (!tokens.HasNext)
                    return new(new VariableExpression(nameIDs));
                if (tokens.Current?.Token == "(")
                {
                    tokens.Pop();
                    var args = new List<AExpression>();
                    if (tokens.Current.Token != ")")
                    {
                        OperationResult<AExpression> argResult = ParseExpression(tokens, parsingContext);
                        if (!argResult)
                            return argResult;
                        args.Add(argResult.Result!);
                        while (tokens.Current?.Token == ",")
                        {
                            tokens.Pop();
                            if (tokens.Current != null)
                            {
                                argResult = ParseExpression(tokens, parsingContext);
                                if (!argResult)
                                    return argResult;
                                args.Add(argResult.Result!);
                            }
                        }
                    }
                    if (tokens.Current?.Token != ")")
                        throw new Exception($"Expected ')' but found '{tokens.Current}'");
                    tokens.Pop();
                    return new(new FunctionCallExpression(nameIDs, args));
                }
                AExpression expr = new VariableExpression(nameIDs);
                if (tokens.Current?.IsUnaryMutation ?? false)
                {
                    string op = tokens.Current.Token;
                    tokens.Pop();
                    return new(new UnaryMutationExpression(expr, op, false));
                }
                return new(expr);
            }
            throw new Exception($"Unexpected token: {tokens.Current}");
        }
    }
}
