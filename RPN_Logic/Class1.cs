using System.Globalization;

namespace RPN_Logic
{
    public abstract class Token { } // Базовый класс для всех токенов
    public class Number : Token // Класс Number, наследуемый от Token
    {
        public double Value { get; }
        public Number(double value)
        {
            Value = value;
        }
    }
    public class Operation : Token // Класс Operation, наследуемый от Token
    {
        public char Symbol { get; }
        public int Priority => Symbol switch
        {
            '*' or '/' => 2,
            '+' or '-' => 1,
            _ => 0
        };
        public Operation(char symbol)
        {
            Symbol = symbol;
        }
    }

    public class Parenthesis : Token // Класс Parenthesis, наследуемый от Token
    {
        public char Symbol { get; }
        public Parenthesis(char symbol)
        {
            Symbol = symbol;
        }
    }

    public static class Calculator
    {
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var currentNum = "";

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if (char.IsDigit(ch) || ch == '.')
                {
                    currentNum += ch;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(currentNum))
                    {
                        tokens.Add(new Number(double.Parse(currentNum, CultureInfo.InvariantCulture)));
                        currentNum = "";
                    }
                    if ("+-*/".Contains(ch))
                    {
                        tokens.Add(new Operation(ch));
                    }
                    else if (ch == '(' || ch == ')')
                    {
                        tokens.Add(new Parenthesis(ch));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(currentNum))
            {
                tokens.Add(new Number(double.Parse(currentNum, CultureInfo.InvariantCulture)));
            }

            return tokens;
        }

        public static List<Token> ConvertToPostfix(List<Token> tokens)
        {
            var postfix = new List<Token>();
            var operationStack = new Stack<Operation>();

            foreach (var token in tokens)
            {
                if (token is Number number)
                {
                    postfix.Add(number);
                }
                else if (token is Operation operation)
                {
                    while (operationStack.Count != 0 && operationStack.Peek().Priority >= operation.Priority)
                    {
                        postfix.Add(operationStack.Pop());
                    }
                    operationStack.Push(operation);
                }
                else if (token is Parenthesis parenthesis)
                {
                    if (parenthesis.Symbol == '(')
                    {
                        operationStack.Push(new Operation(parenthesis.Symbol));
                    }
                    else
                    {
                        while (operationStack.Count > 0 && operationStack.Peek().Symbol != '(')
                        {
                            postfix.Add(operationStack.Pop());
                        }
                        if (operationStack.Count > 0 && operationStack.Peek().Symbol == '(')
                        {
                            operationStack.Pop();
                        }
                    }
                }
            }

            while (operationStack.Count != 0)
            {
                postfix.Add(operationStack.Pop());
            }

            return postfix;
        }

        public static double EvaluatePostfix(List<Token> postfix)
        {
            var values = new Stack<double>();

            foreach (var token in postfix)
            {
                if (token is Number number)
                {
                    values.Push(number.Value);
                }
                else if (token is Operation operation)
                {
                    double a = values.Pop();
                    double b = values.Pop();
                    values.Push(ApplyOperation(operation.Symbol, b, a));
                }
            }

            return values.Pop();
        }

        private static double ApplyOperation(char op, double a, double b)
        {
            switch (op)
            {
                case '+': return a + b;
                case '-': return a - b;
                case '*': return a * b;
                case '/':
                    if (b == 0)
                        throw new DivideByZeroException("Попытка деления на ноль.");
                    return a / b;
                default:
                    throw new ArgumentException($"Неподдерживаемая операция: {op}");
            }
        }
    }
}
