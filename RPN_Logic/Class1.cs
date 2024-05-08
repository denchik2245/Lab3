using System.Globalization;
using System.Text;

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

        public override string ToString() => Value.ToString();
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

        public override string ToString() => Symbol.ToString();
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

        public override string ToString() => Name;
    }

    
    public class Variable : Token
    {
        public string Name { get; }

        public Variable(string name)
        {
            Name = name;
        }

        public override string ToString() => Name;
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
        public Associativity Associativity => Symbol switch
        {
            '^' => Associativity.Right,
            '*' or '/' or '+' or '-' => Associativity.Left,
            _ => Associativity.Left
        };

        public Operation(char symbol)
        {
            Symbol = symbol;
        }

        public override string ToString() => Symbol.ToString();
    }

    public enum Associativity
    {
        Left,
        Right
    }
    
    public class Parenthesis : Token
    {
        public char Symbol { get; }

        public Parenthesis(char symbol)
        {
            Symbol = symbol;
        }

        public override string ToString() => Symbol.ToString();
    }


    public static class Calculator
    {
        //Разбиваем на список токенов
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var currentNum = new StringBuilder();
            var currentVar = new StringBuilder();

            //Проходим по каждому символу во входной строке
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                
                if (char.IsDigit(ch) || ch == '.')
                {
                    currentNum.Append(ch);
                }
                else
                {
                    if (currentNum.Length > 0)
                    {
                        tokens.Add(new Number(double.Parse(currentNum.ToString(), CultureInfo.InvariantCulture)));
                        currentNum.Clear();
                    }
                    if (char.IsLetter(ch))
                    {
                        currentVar.Append(ch);
                    }
                    else
                    {
                        if (currentVar.Length > 0)
                        {
                            ProcessVariable(currentVar.ToString(), tokens);
                            currentVar.Clear();
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
            if (currentNum.Length > 0)
            {
                tokens.Add(new Number(double.Parse(currentNum.ToString(), CultureInfo.InvariantCulture)));
            }
            if (currentVar.Length > 0)
            {
                ProcessVariable(currentVar.ToString(), tokens);
            }

            return tokens;
        }

        //Метод для обработки переменной как функции или переменной
        private static void ProcessVariable(string var, List<Token> tokens)
        {
            try
            {
                int argsCount = FunctionArgumentsCount(var);
                tokens.Add(new Function(var, argsCount));
            }
            catch (ArgumentException)
            {
                tokens.Add(new Variable(var));
            }
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

        //Метод для преобразования инфиксного выражения в постфиксное
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

                    case Variable variable:
                        postfix.Add(variable);
                        break;

                    case Function function:
                        operationStack.Push(function);
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
                               ((operation.Associativity == Associativity.Left && topOperation.Priority >= operation.Priority) ||
                                (operation.Associativity == Associativity.Right && topOperation.Priority > operation.Priority)))
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
                        
                        if (operationStack.Count > 0 && operationStack.Peek() is Function)
                        {
                            postfix.Add(operationStack.Pop());
                        }
                        break;
                }
            }

            // Переместить оставшиеся операции из стека в постфиксную запись
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

        //Считаем выражение
        public static double EvaluatePostfix(List<Token> postfix, Dictionary<string, double> variableValues)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                switch (token)
                {
                    case Number number:
                        stack.Push(number.Value);
                        break;

                    case Variable variable:
                        if (variableValues.TryGetValue(variable.Name, out var value))
                        {
                            stack.Push(value);
                        }
                        else
                        {
                            throw new ArgumentException($"Переменная '{variable.Name}' не определена.");
                        }
                        break;

                    case Operation operation:
                        if (stack.Count < 2)
                        {
                            throw new InvalidOperationException("Недостаточно операндов в стеке для бинарной операции.");
                        }
                        var b = stack.Pop();
                        var a = stack.Pop();
                        stack.Push(ApplyOperation(operation.Symbol, a, b));
                        break;

                    case UnaryOperation unaryOperation:
                        if (stack.Count < 1)
                        {
                            throw new InvalidOperationException("Недостаточно операндов в стеке для унарной операции.");
                        }
                        var operand = stack.Pop();
                        stack.Push(ApplyUnaryOperation(unaryOperation.Symbol, operand));
                        break;

                    case Function function:
                        if (stack.Count < function.ArgumentsCount)
                        {
                            throw new InvalidOperationException($"Недостаточно аргументов для функции '{function.Name}'.");
                        }
                        var arguments = new double[function.ArgumentsCount];
                        for (int i = function.ArgumentsCount - 1; i >= 0; i--)
                        {
                            arguments[i] = stack.Pop();
                        }
                        stack.Push(ApplyFunction(function, arguments));
                        break;

                    default:
                        throw new InvalidOperationException($"Неизвестный токен '{token}'.");
                }
            }

            if (stack.Count != 1)
            {
                throw new InvalidOperationException("Неверное количество значений в стеке после вычисления.");
            }

            return stack.Pop();
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