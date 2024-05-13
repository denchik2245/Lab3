using System.Globalization;
using System.Text;

namespace RPN_Logic
{
    public abstract class Token { }
    public class Number : Token
    {
        public double Value { get; }
        public Number(double value) => Value = value;
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public class UnaryOperation : Token
    {
        public char Symbol { get; }
        public int Priority { get; }
        public UnaryOperation(char symbol, int priority) => (Symbol, Priority) = (symbol, priority);
        public override string ToString() => Symbol.ToString();
    }

    public class Function : Token
    {
        public string Name { get; }
        public int ArgumentsCount { get; }
        public Function(string name, int argumentsCount) => (Name, ArgumentsCount) = (name, argumentsCount);
        public override string ToString() => Name;
    }
    
    public class Variable : Token
    {
        public string Name { get; }
        public Variable(string name) => Name = name;
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
        public Operation(char symbol) => Symbol = symbol;
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
        public Parenthesis(char symbol) => Symbol = symbol;
        public override string ToString() => Symbol.ToString();
    }
    
    public class Comma : Token
    {
        public override string ToString() => ",";
    }

    public static class Calculator
    {
        //Разбиваем на список токенов
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var currentNum = new StringBuilder();
            var currentVar = new StringBuilder();
            
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
                        if (!char.IsWhiteSpace(ch) && ch != '(' && ch != ')' && ch != ',')
                        {
                            tokens.Add(new Operation(ch));
                        }
                        else if (ch == '(' || ch == ')')
                        {
                            tokens.Add(new Parenthesis(ch));
                        }
                        else if (ch == ',')
                        {
                            tokens.Add(new Comma());
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
                    case Variable variable:
                        postfix.Add(token);
                        break;

                    case Function function:
                    case Parenthesis parenthesis when parenthesis.Symbol == '(':
                        operationStack.Push(token);
                        break;

                    case Comma:
                        MoveOperatorsUntilParenthesis(operationStack, postfix);
                        break;

                    case UnaryOperation unaryOperation:
                    case Operation operation:
                        MoveOperatorsUntilLowerPriority(operationStack, postfix, token);
                        operationStack.Push(token);
                        break;

                    case Parenthesis parenthesis when parenthesis.Symbol == ')':
                        MoveOperatorsUntilParenthesis(operationStack, postfix);
                        PopFunctionIfPresent(operationStack, postfix);
                        break;
                }
            }

            postfix.AddRange(operationStack.Where(t => !(t is Parenthesis)));
            return postfix;
        }

        private static void MoveOperatorsUntilParenthesis(Stack<Token> operationStack, List<Token> postfix)
        {
            while (operationStack.Count > 0 && !(operationStack.Peek() is Parenthesis p && p.Symbol == '('))
            {
                postfix.Add(operationStack.Pop());
            }
        }

        private static void MoveOperatorsUntilLowerPriority(Stack<Token> operationStack, List<Token> postfix, Token token)
        {
            while (operationStack.Count != 0 &&
                   operationStack.Peek() is Operation topOperation &&
                   ((token is UnaryOperation && topOperation.Priority >= ((UnaryOperation)token).Priority) ||
                    (token is Operation && ((Operation)token).Associativity == Associativity.Left && topOperation.Priority >= ((Operation)token).Priority) ||
                    (token is Operation && ((Operation)token).Associativity == Associativity.Right && topOperation.Priority > ((Operation)token).Priority)))
            {
                postfix.Add(operationStack.Pop());
            }
        }

        private static void PopFunctionIfPresent(Stack<Token> operationStack, List<Token> postfix)
        {
            if (operationStack.Count > 0 && operationStack.Peek() is Parenthesis)
            {
                operationStack.Pop();
            }
            if (operationStack.Count > 0 && operationStack.Peek() is Function)
            {
                postfix.Add(operationStack.Pop());
            }
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
            return function.Name switch
            {
                "log" => args.Length == 2 ? Math.Log(args[1], args[0]) : throw new ArgumentException("Function 'log' expects two arguments."),
                "rt" => args.Length == 2 ? Math.Pow(args[1], 1 / args[0]) : throw new ArgumentException("Function 'rt' expects two arguments."),
                "sqrt" => args.Length == 1 ? Math.Sqrt(args[0]) : throw new ArgumentException("Function 'sqrt' expects one argument."),
                "sin" => args.Length == 1 ? Math.Sin(args[0]) : throw new ArgumentException("Function 'sin' expects one argument."),
                "cos" => args.Length == 1 ? Math.Cos(args[0]) : throw new ArgumentException("Function 'cos' expects one argument."),
                "tg" => args.Length == 1 ? Math.Tan(args[0]) : throw new ArgumentException("Function 'tg' expects one argument."),
                "ctg" => args.Length == 1 ? 1 / Math.Tan(args[0]) : throw new ArgumentException("Function 'ctg' expects one argument."),
                _ => throw new ArgumentException($"Unsupported function: {function.Name}")
            };
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