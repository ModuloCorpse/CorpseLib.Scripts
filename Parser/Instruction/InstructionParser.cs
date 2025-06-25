using CorpseLib.Scripts.Parser.Instruction.Expressions;

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
            return [..ids];
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
                string typeStr = currentToken.Token;
                OperationResult<TypeInfo> typeInfoResult = TypeInfo.ParseStr(typeStr, parsingContext.ConversionTable);
                if (!typeInfoResult)
                    return typeInfoResult.Cast<AExpression>();
                TypeInfo type = typeInfoResult.Result!;
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
                BinaryExpression binaryExpression = new(op, new VariableExpression(nameIDs), exprResult.Result!);
                return new(new AssignmentExpression(null, nameIDs, binaryExpression));
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

        private static OperationResult<AExpression> ParseExpression(TokenReader tokens, ParsingContext parsingContext, int weight = 0)
        {
            OperationResult<AExpression> leftResult = ParsePrimary(tokens, parsingContext, false);
            if (!leftResult)
                return leftResult;
            AExpression left = leftResult.Result!;
            while (tokens.HasNext)
            {
                ExpressionToken nextToken = tokens.Current!;
                string op = nextToken.Token;
                int tokenWeight = nextToken.Weight;
                if (tokenWeight < weight || tokenWeight < 0)
                    break;
                tokens.Pop();
                int nextWeight = tokenWeight + (op == "=" ? 0 : 1);
                OperationResult<AExpression> rightResult = ParseExpression(tokens, parsingContext, nextWeight);
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

        private static bool IsExpressionLiteral(AExpression expr)
        {
            if (expr is LiteralExpression)
                return true;
            else if (expr is AnonymousObjectExpression anonymousObjectExpression)
            {
                foreach (AExpression parameter in anonymousObjectExpression.Parameters)
                {
                    if (!IsExpressionLiteral(parameter))
                        return false;
                }
                return true;
            }
            return false;
        }

        private static OperationResult<AExpression> ParsePrimary(TokenReader tokens, ParsingContext parsingContext, bool isReversed)
        {
            ExpressionToken? currentToken = tokens.Current;
            if (currentToken == null)
                return new("Error while parsing instruction", "tokens are empty");
            if (currentToken.IsUnaryMutation)
            {
                string op = currentToken.Token;
                tokens.Pop();
                OperationResult<AExpression> target = ParsePrimary(tokens, parsingContext, false);
                if (!target)
                    return target;
                return new(new UnaryMutationExpression(target.Result!, op, true));
            }
            else if (currentToken.IsUnaryOperator)
            {
                string op = currentToken.Token;
                tokens.Pop();
                bool isNegative = (op == "-");
                OperationResult<AExpression> operandResult = ParsePrimary(tokens, parsingContext, isNegative);
                if (!operandResult)
                    return operandResult;
                AExpression operand = operandResult.Result!;
                if (isNegative && operand is LiteralExpression literal && literal.Value.Length == 1 && literal.Value[0] is not string)
                    return new(operand);
                return new(new UnaryExpression(op, operand));
            }
            else if (tokens.Current?.Token == "{")
            {
                tokens.Pop();
                List<AExpression> parameters = [];
                if (tokens.Current.Token != "}")
                {
                    OperationResult<AExpression> parameterResult = ParseExpression(tokens, parsingContext);
                    if (!parameterResult)
                        return parameterResult;
                    parameters.Add(parameterResult.Result!);
                    while (tokens.Current?.Token == ",")
                    {
                        tokens.Pop();
                        if (tokens.Current != null)
                        {
                            parameterResult = ParseExpression(tokens, parsingContext);
                            if (!parameterResult)
                                return parameterResult;
                            parameters.Add(parameterResult.Result!);
                        }
                    }
                }
                if (tokens.Current?.Token != "}")
                    return new("Error while parsing instruction", $"Expected '}}' but found '{tokens.Current}'");
                tokens.Pop();
                //TODO Handle when AnonymousObjectExpression is composed only of literals
                return new(new AnonymousObjectExpression(parameters, false));
            }
            else if (tokens.Current?.Token == "[")
            {
                tokens.Pop();
                List<AExpression> parameters = [];
                if (tokens.Current.Token != "]")
                {
                    OperationResult<AExpression> parameterResult = ParseExpression(tokens, parsingContext);
                    if (!parameterResult)
                        return parameterResult;
                    parameters.Add(parameterResult.Result!);
                    while (tokens.Current?.Token == ",")
                    {
                        tokens.Pop();
                        if (tokens.Current != null)
                        {
                            parameterResult = ParseExpression(tokens, parsingContext);
                            if (!parameterResult)
                                return parameterResult;
                            parameters.Add(parameterResult.Result!);
                        }
                    }
                }
                if (tokens.Current?.Token != "]")
                    return new("Error while parsing instruction", $"Expected ']' but found '{tokens.Current}'");
                tokens.Pop();
                //TODO Handle when AnonymousObjectExpression is composed only of literals
                return new(new AnonymousObjectExpression(parameters, true));
            }
            else if (currentToken.Token == "(")
            {
                tokens.Pop();
                OperationResult<AExpression> expr = ParseExpression(tokens, parsingContext);
                if (tokens.Current?.Token != ")")
                    return new("Error while parsing instruction", $"Expected ')' but found '{tokens.Current}'");
                tokens.Pop();
                return expr;
            }
            else if (currentToken.IsLiteral)
            {
                string literal = currentToken.Token;
                if (isReversed)
                    literal = $"-{literal}";
                tokens.Pop();
                return new(new LiteralExpression(ValueParser.ParseValue(literal, parsingContext)));
            }
            else if (currentToken.IsIdentifier)
            {
                int[] nameIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                tokens.Pop();
                if (!tokens.HasNext)
                    return new(new VariableExpression(nameIDs));
                List<int[]> templates = [];
                if (tokens.Current?.Token == "<")
                {
                    tokens.Pop();
                    if (tokens.Current.Token != ">")
                    {
                        int[] templateIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                        templates.Add(templateIDs);
                        while (tokens.Current?.Token == ",")
                        {
                            tokens.Pop();
                            if (tokens.Current != null)
                            {
                                templateIDs = ConvertNameFromToken(tokens.Current!, parsingContext);
                                templates.Add(templateIDs);
                            }
                        }
                    }
                    if (tokens.Current?.Token != ">")
                        return new("Error while parsing instruction", $"Expected '>' but found '{tokens.Current}'");
                    tokens.Pop();
                }

                if (tokens.Current?.Token == "(")
                {
                    tokens.Pop();
                    List<AExpression> args = [];
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
                        return new("Error while parsing instruction", $"Expected ')' but found '{tokens.Current}'");
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
            return new("Error while parsing instruction", $"Unexpected token: {tokens.Current}");
        }
    }
}
