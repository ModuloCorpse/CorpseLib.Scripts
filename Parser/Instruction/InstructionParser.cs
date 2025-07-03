using CorpseLib.Scripts.Instructions;
using CorpseLib.Scripts.Operations;
using CorpseLib.Scripts.Parser.Instruction.Expressions;

namespace CorpseLib.Scripts.Parser.Instruction
{
    public partial class InstructionParser
    {
        private static OperationResult<AExpression> ParseExpression(TokenReader tokens, ParsingContext parsingContext, int weight = 0)
        {
            OperationResult<AExpression> leftResult = ParseTernaryExpression(tokens, parsingContext, false);
            if (!leftResult)
                return leftResult;
            AExpression left = leftResult.Result!;
            while (tokens.HasNext)
            {
                ExpressionToken nextToken = tokens.Current!;
                bool isAssignment = (nextToken.Token == "=");
                int tokenWeight = nextToken.Weight;
                if (tokenWeight < weight || tokenWeight < 0)
                    break;
                tokens.Pop();
                int nextWeight = tokenWeight + (isAssignment ? 0 : 1);
                OperationResult<AExpression> rightResult = ParseExpression(tokens, parsingContext, nextWeight);
                if (!rightResult)
                    return rightResult;
                AExpression right = rightResult.Result!;
                if (isAssignment)
                    left = new AssignmentExpression(left, right);
                else if (nextToken.IsCompoundOperator && nextToken.Token[^1] == '=')
                {
                    BinaryExpression binaryExpression = new(nextToken.Operator, left, right);
                    left = new AssignmentExpression(left, binaryExpression);
                }
                else
                    left = new BinaryExpression(nextToken.Operator, left, right);
            }
            return new(left);
        }

        private static object[] ConvertAnonymousObject(AExpression expression)
        {
            if (expression is AnonymousObjectExpression anonymousObject)
            {
                List<object[]> values = [];
                foreach (AExpression parameter in anonymousObject.Parameters)
                    values.Add(ConvertAnonymousObject(parameter));
                return anonymousObject.IsArray ? [values] : [..values];
            }
            return (expression as LiteralExpression)!.Value;
        }

        private static bool IsAnonymousObjectALiteral(AExpression expr)
        {
            if (expr is LiteralExpression)
                return true;
            else if (expr is AnonymousObjectExpression anonymousObjectExpression)
            {
                foreach (AExpression parameter in anonymousObjectExpression.Parameters)
                {
                    if (!IsAnonymousObjectALiteral(parameter))
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
                Operator op = currentToken.Operator;
                tokens.Pop();
                OperationResult<AExpression> target = ParseTernaryExpression(tokens, parsingContext, false);
                if (!target)
                    return target;
                return new(new MutationExpression(target.Result!, op, true));
            }
            else if (currentToken.IsUnaryOperator)
            {
                tokens.Pop();
                Operator op = currentToken.Operator;
                bool isNegative = (currentToken.Token == "-");
                OperationResult<AExpression> operandResult = ParseTernaryExpression(tokens, parsingContext, isNegative);
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
                int nameID = parsingContext.ConversionTable.PushName(tokens.Current!.Token);
                tokens.Pop();
                if (!tokens.HasNext)
                    return new(new VariableExpression(nameID));

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
                    //TODO
                    return new(new FunctionCallExpression([nameID], args));
                }
                return new(new VariableExpression(nameID));
            }
            return new("Error while parsing instruction", $"Unexpected token: {tokens.Current}");
        }

        private static OperationResult<AExpression> ParseTernaryExpression(TokenReader tokens, ParsingContext parsingContext, bool isReversed)
        {
            OperationResult<AExpression> result = ParsePrimary(tokens, parsingContext, isReversed);
            if (!result)
                return result;
            if (result.Result is AnonymousObjectExpression anonymousObject && IsAnonymousObjectALiteral(anonymousObject))
                return new(new LiteralExpression(ConvertAnonymousObject(anonymousObject)));

            if (tokens.Current?.IsUnaryMutation ?? false)
            {
                Operator op = tokens.Current.Operator;
                tokens.Pop();
                return new(new MutationExpression(result.Result!, op, false));
            }
            else if (tokens.Current?.Token == "[")
            {
                do
                {
                    tokens.Pop();
                    OperationResult<AExpression> indexResult = ParseTernaryExpression(tokens, parsingContext, false);
                    if (!indexResult)
                        return indexResult;
                    if (tokens.Current?.Token != "]")
                        return new("Error while parsing instruction", $"Expected ')' but found '{tokens.Current}'");
                    tokens.Pop();
                    result = new(new ArrayAccessExpression(result.Result!, indexResult.Result!));
                } while (tokens.Current?.Token == "[");
                return result;
            }
            else if (tokens.Current?.Token == ".")
            {
                while (tokens.Current?.Token is "." or "[")
                {
                    if (tokens.Current?.Token == ".")
                    {
                        tokens.Pop();
                        // Un accès membre DOIT être suivi d’un identifiant
                        if (tokens.Current is not { IsIdentifier: true })
                            return new("Error while parsing instruction", $"Expected identifier after '.' but found '{tokens.Current?.Token}'");

                        int memberID = parsingContext.ConversionTable.PushName(tokens.Current!.Token);
                        tokens.Pop();
                        result = new(new MemberAccessExpression(result.Result!, new VariableExpression(memberID)));
                    }
                    else if (tokens.Current?.Token == "[")
                    {
                        tokens.Pop();
                        OperationResult<AExpression> indexResult = ParseExpression(tokens, parsingContext);
                        if (!indexResult)
                            return indexResult;
                        if (tokens.Current?.Token != "]")
                            return new("Error while parsing instruction", $"Expected ']' but found '{tokens.Current?.Token}'");
                        tokens.Pop();
                        result = new(new ArrayAccessExpression(result.Result!, indexResult.Result!));
                    }
                }
                return result;
            }
            else if (tokens.Current?.Token == "?")
            {
                tokens.Pop();
                OperationResult<AExpression> ternaryResult = ParseExpression(tokens, parsingContext);
                if (!ternaryResult)
                    return ternaryResult;
                if (ternaryResult.Result is BinaryExpression binary)
                    return new(new TernaryExpression(result.Result!, binary.Left, binary.Right));
                return new("Error while parsing instruction", "Invalid ternary");
            }
            return result;
        }

        private static OperationResult<AExpression> ParseExpressions(string input, ParsingContext parsingContext)
        {
            if (string.IsNullOrEmpty(input))
                return new("Error while parsing instruction", "Input is empty");
            TokenReader tokens = new(input);
            ExpressionToken currentToken = tokens.Current!;
            if (currentToken.IsIdentifier && (tokens[1]?.IsIdentifier ?? false) && (tokens[2]?.Token == "=" || tokens[2] == null))
            {
                string typeStr = currentToken.Token;
                OperationResult<TypeInfo> typeInfoResult = TypeInfo.ParseStr(typeStr, parsingContext);
                if (!typeInfoResult)
                    return typeInfoResult.Cast<AExpression>();
                TypeInfo type = typeInfoResult.Result!;
                tokens.Pop();
                int nameID = parsingContext.ConversionTable.PushName(tokens.Current!.Token);
                tokens.Pop();
                if (tokens.Current?.Token == "=")
                {
                    tokens.Pop();
                    OperationResult<AExpression> exprResult = ParseExpression(tokens, parsingContext);
                    if (!exprResult)
                        return exprResult;
                    AExpression expr = exprResult.Result!;
                    return new(new CreateVariableExpression(type, nameID, expr));
                }
                return new(new CreateVariableExpression(type, nameID, null));
            }
            else if (currentToken.IsUnaryMutation && (tokens[1]?.IsIdentifier ?? false) && tokens[2] == null)
            {
                Operator op = currentToken.Operator;
                tokens.Pop();
                OperationResult<AExpression> exprResult = ParseExpression(tokens, parsingContext);
                if (!exprResult)
                    return exprResult;
                AExpression expr = exprResult.Result!;
                return new(new MutationExpression(expr, op, true));
            }
            return ParseExpression(tokens, parsingContext);
        }

        private static AOperationTreeNode? ConvertExpressions(AExpression expression, ParsingContext parsingContext)
        {
            if (expression is LiteralExpression literal)
                return new LiteralOperationNode(literal.Value);
            return null;
        }

        public static AInstruction? Parse(string instruction, ParsingContext parsingContext)
        {
            if (instruction == "break")
                return new Break();
            else if (instruction == "continue")
                return new Continue();
            else if (instruction == "return")
                return new ReturnInstruction(null);
            bool isReturn = instruction.StartsWith("return ");
            if (isReturn)
                instruction = instruction[7..];
            OperationResult<AExpression> result = ParseExpressions(instruction, parsingContext);
            if (!result)
            {
                parsingContext.RegisterError($"Cannot parse '{instruction}'", result.Description);
                return null;
            }
            AExpression expression = result.Result!;
            if (!expression.HasSideEffects && !isReturn)
            {
                parsingContext.RegisterWarning("Optimized away", $"The expression '{instruction}' was optimized away.");
                expression.Dump(parsingContext.ConversionTable);
                return null;
            }
            AOperationTreeNode? operationTree = ConvertExpressions(expression, parsingContext);
            if (operationTree == null)
            {
                parsingContext.RegisterWarning("No operation tree", $"The expression '{instruction}' cannot be converted to operation tree.");
                return new DebugInstruction(expression, instruction);
            }
            else
            {
                if (!operationTree.Validate(parsingContext, instruction))
                    return null;

                if (isReturn)
                    return new ReturnInstruction(operationTree);
                else
                    return new OperationInstruction(operationTree);
            }
        }
    }
}
