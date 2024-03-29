using System.Globalization;

namespace RPN_Logic
{
    public abstract class Token { }
    public class Number : Token
    {
        public double Value { get; }
        public Number(double value)
        {
            Value = value;
        }
    }

    public class Variable : Token
    {
        public string Name { get; }
        public Variable(string name)
        {
            Name = name;
        }
    }

    public class Operation : Token
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

    public class Parenthesis : Token
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
                    else if (char.IsLetter(ch))
                    {
                        tokens.Add(new Variable(ch.ToString()));
                    }
                    else if (!char.IsWhiteSpace(ch))
                    {
                        throw new ArgumentException($"Неподдерживаемый символ: {ch}");
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
            var operationStack = new Stack<Token>();

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case Number number:
                        postfix.Add(number);
                        break;
                    case Operation operation:
                        while (operationStack.Count != 0 &&
                               operationStack.Peek() is Operation topOperation &&
                               topOperation.Priority >= operation.Priority)
                        {
                            postfix.Add(operationStack.Pop());
                        }
                        operationStack.Push(operation);
                        break;
                    case Parenthesis parenthesis when parenthesis.Symbol == '(':
                        operationStack.Push(parenthesis);
                        break;
                    case Parenthesis parenthesis when parenthesis.Symbol == ')':
                        while (operationStack.Count > 0 && !(operationStack.Peek() is Parenthesis p && p.Symbol == '('))
                        {
                            postfix.Add(operationStack.Pop());
                        }
                        if (operationStack.Count > 0 && operationStack.Peek() is Parenthesis)
                        {
                            operationStack.Pop();
                        }
                        break;
                    case Variable variable:
                        postfix.Add(variable);
                        break;
                }
            }

            while (operationStack.Count != 0)
            {
                var remainingToken = operationStack.Pop();
                if (!(remainingToken is Parenthesis))
                {
                    postfix.Add(remainingToken);
                }
            }

            return postfix;
        }

        public static double EvaluatePostfix(List<Token> postfix, Dictionary<string, double> variableValues)
        {
            var values = new Stack<double>();

            foreach (var token in postfix)
            {
                switch (token)
                {
                    case Number number:
                        values.Push(number.Value);
                        break;
                    case Operation operation:
                        if (values.Count < 2)
                            throw new InvalidOperationException("Недостаточно операндов в стеке для выполнения операции.");

                        double b = values.Pop();
                        double a = values.Pop();

                        values.Push(ApplyOperation(operation.Symbol, a, b));
                        break;
                    case Variable variable:
                        if (!variableValues.TryGetValue(variable.Name, out var value))
                            throw new ArgumentException($"Неизвестная переменная: {variable.Name}");
                        values.Push(value);
                        break;
                }
            }

            if (values.Count != 1)
                throw new InvalidOperationException("Ошибка в выражении: в стеке осталось более одного значения.");

            return values.Pop();
        }


        private static double ApplyOperation(char op, double a, double b)
        {
            return op switch
            {
                '+' => a + b,
                '-' => a - b,
                '*' => a * b,
                '/' => b == 0 ? throw new DivideByZeroException("Попытка деления на ноль.") : a / b,
                _ => throw new ArgumentException($"Неподдерживаемая операция: {op}")
            };
        }
    }
}