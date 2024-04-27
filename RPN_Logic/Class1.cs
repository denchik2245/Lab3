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

    public class UnaryOperation : Token
    {
        public char Symbol { get; }
        public int Priority { get; } 

        public UnaryOperation(char symbol, int priority)
        {
            Symbol = symbol;
            Priority = priority;
        }
    }

    public class Function : Token
    {
        public string Name { get; }
        public int ArgumentsCount { get; }

        public Function(string name, int argumentsCount)
        {
            Name = name;
            ArgumentsCount = argumentsCount;
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
            '^' => 3,
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
            var currentVar = "";

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if (char.IsDigit(ch) || ch == '.')
                {
                    currentNum += ch;
                }
                else
                {
                    if (currentNum != "")
                    {
                        tokens.Add(new Number(double.Parse(currentNum, CultureInfo.InvariantCulture)));
                        currentNum = "";
                    }

                    if (char.IsLetter(ch))
                    {
                        currentVar += ch;
                    }
                    else
                    {
                        if (currentVar != "")
                        {
                            try
                            {
                                int argsCount = FunctionArgumentsCount(currentVar);
                                tokens.Add(new Function(currentVar, argsCount));
                            }
                            catch (ArgumentException)
                            {
                                tokens.Add(new Variable(currentVar));
                            }
                            currentVar = "";
                        }

                        if (!char.IsWhiteSpace(ch) && ch != '(' && ch != ')')
                        {
                            tokens.Add(new Operation(ch));
                        }
                        else if (ch == '(' || ch == ')')
                        {
                            tokens.Add(new Parenthesis(ch));
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentNum))
            {
                tokens.Add(new Number(double.Parse(currentNum, CultureInfo.InvariantCulture)));
            }
            if (!string.IsNullOrEmpty(currentVar))
            {
                try
                {
                    int argsCount = FunctionArgumentsCount(currentVar);
                    tokens.Add(new Function(currentVar, argsCount));
                }
                catch (ArgumentException)
                {
                    tokens.Add(new Variable(currentVar));
                }
            }

            return tokens;
        }
        
        private static int FunctionArgumentsCount(string functionName)
        {
            return functionName switch
            {
                "log" => 2,
                "rt" => 2,
                "sqrt" => 1,
                "sin" => 1,
                "cos" => 1,
                "tg" => 1,
                "ctg" => 1,
                _ => throw new ArgumentException($"Unsupported function: {functionName}")
            };
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
                    case UnaryOperation unaryOperation:
                        while (operationStack.Count != 0 &&
                               operationStack.Peek() is Operation topOperation &&
                               topOperation.Priority >= unaryOperation.Priority)
                        {
                            postfix.Add(operationStack.Pop());
                        }
                        operationStack.Push(unaryOperation);
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
            double[] args = null;

            foreach (var token in postfix)
            {
                switch (token)
                {
                    case Number number:
                        values.Push(number.Value);
                        break;

                    case UnaryOperation unaryOperation:
                        double unaryValue = values.Pop();
                        values.Push(ApplyUnaryOperation(unaryOperation.Symbol, unaryValue));
                        break;

                    case Operation operation:
                        double b = values.Pop();
                        double a = values.Pop();
                        values.Push(ApplyOperation(operation.Symbol, a, b));
                        break;

                    case Function function:
                        args = new double[function.ArgumentsCount];
                        for (int i = function.ArgumentsCount - 1; i >= 0; i--)
                        {
                            args[i] = values.Pop();
                        }
                        values.Push(ApplyFunction(function, args));
                        break;

                    case Variable variable:
                        if (!variableValues.TryGetValue(variable.Name, out double varValue))
                            throw new ArgumentException($"Unknown variable: {variable.Name}");
                        values.Push(varValue);
                        break;
                }
            }

            if (values.Count != 1)
                throw new InvalidOperationException("Error in expression: More than one value left in the stack.");

            return values.Pop();
        }
        
        private static double ApplyFunction(Function function, double[] args)
        {
            switch (function.Name)
            {
                case "log":
                    if (args.Length != 2)
                        throw new ArgumentException("Function 'log' expects two arguments.");
                    return Math.Log(args[0], args[1]);
                case "rt":
                    if (args.Length != 2)
                        throw new ArgumentException("Function 'rt' expects two arguments.");
                    return Math.Pow(args[1], 1 / args[0]);
                case "sqrt":
                    if (args.Length != 1)
                        throw new ArgumentException("Function 'sqrt' expects one argument.");
                    if (args[0] < 0)
                        throw new ArgumentException("Cannot calculate square root of a negative number.");
                    return Math.Sqrt(args[0]);
                case "sin":
                case "cos":
                case "tg":
                case "ctg":
                    if (args.Length != 1)
                        throw new ArgumentException($"Function '{function.Name}' expects one argument.");
                    return function.Name switch
                    {
                        "sin" => Math.Sin(args[0]),
                        "cos" => Math.Cos(args[0]),
                        "tg" => Math.Tan(args[0]),
                        "ctg" => 1 / Math.Tan(args[0]),
                        _ => throw new ArgumentException($"Unsupported function: {function.Name}")
                    };
                default:
                    throw new ArgumentException($"Unsupported function: {function.Name}");
            }
        }

        private static double ApplyUnaryOperation(char op, double a)
        {
            return op switch
            {
                '-' => -a,
                _ => throw new ArgumentException($"Неподдерживаемая унарная операция: {op}")
            };
        }

        private static double ApplyOperation(char op, double a, double b)
        {
            return op switch
            {
                '+' => a + b,
                '-' => a - b,
                '*' => a * b,
                '/' => b == 0 ? throw new DivideByZeroException("Attempt to divide by zero.") : a / b,
                '^' => Math.Pow(a, b),
                _ => throw new ArgumentException($"Unsupported operation: {op}")
            };
        }
    }
}