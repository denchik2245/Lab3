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
        //Метод для разбиения выражения на токены
        public static List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (char.IsDigit(ch) || ch == '.')
                {
                    var currentNum = new StringBuilder();
                    while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.'))
                    {
                        currentNum.Append(input[i]);
                        i++;
                    }
                    tokens.Add(new Number(double.Parse(currentNum.ToString(), CultureInfo.InvariantCulture)));
                    i--;
                }
                else if (char.IsLetter(ch))
                {
                    var currentVar = new StringBuilder();
                    while (i < input.Length && char.IsLetter(input[i]))
                    {
                        currentVar.Append(input[i]);
                        i++;
                    }
                    ProcessVariable(currentVar.ToString(), tokens);
                    i--;
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    if (ch == '(' || ch == ')')
                    {
                        tokens.Add(new Parenthesis(ch));
                    }
                    else if (ch == ',')
                    {
                        tokens.Add(new Comma());
                    }
                    else
                    {
                        tokens.Add(new Operation(ch));
                    }
                }
            }
            return tokens;
        }

        //Метод для определения это Функция или переменная
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

        //Метод для вывода количества операторов в зависимости от функции
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
                _ => throw new ArgumentException($"Неподдерживаемая функция: {functionName}")
            };
        }

        //Метод для составления ОПЗ из токенов
        public static List<Token> ConvertToPostfix(List<Token> tokens)
        {
            var postfix = new List<Token>();
            var operationStack = new Stack<Token>();
            bool expectOperand = true;

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case Number:
                    case Variable:
                        postfix.Add(token);
                        expectOperand = false;
                        break;

                    case Function:
                    case Parenthesis parenthesis when parenthesis.Symbol == '(':
                        operationStack.Push(token);
                        expectOperand = true;
                        break;

                    case Comma:
                        MoveOperatorsUntilParenthesis(operationStack, postfix);
                        break;

                    case Operation operation when expectOperand && (operation.Symbol == '-' || operation.Symbol == '+'):
                        operationStack.Push(new UnaryOperation(operation.Symbol, 4));
                        break;

                    case UnaryOperation:
                    case Operation:
                        MoveOperatorsUntilLowerPriority(operationStack, postfix, token);
                        operationStack.Push(token);
                        expectOperand = true;
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
        
        //Метод для перемещения операторов в список ОПЗ пока не встретится открывающая скобка
        private static void MoveOperatorsUntilParenthesis(Stack<Token> operationStack, List<Token> postfix)
        {
            while (operationStack.Count > 0 && !(operationStack.Peek() is Parenthesis p && p.Symbol == '('))
            {
                postfix.Add(operationStack.Pop());
            }
        }

        //Метод для перемещения операторов в список ОПЗ в зависимости от приоритета
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

        //Метод для скобок и функций
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

        //Метод для подсчета всего выражения, записанного в ОПЗ
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

        //Метод для подсчета функций
        private static double ApplyFunction(Function function, double[] args)
        {
            return function.Name switch
            {
                "log" => args.Length == 2 ? Math.Log(args[1], args[0]) : throw new ArgumentException("Функция 'log' должна иметь 2 аргумента."),
                "rt" => args.Length == 2 ? Math.Pow(args[1], 1 / args[0]) : throw new ArgumentException("Функция 'rt' должна иметь 2 аргумента."),
                "sqrt" => args.Length == 1 ? Math.Sqrt(args[0]) : throw new ArgumentException("Функция 'sqrt' должна иметь 1 аргумент."),
                "sin" => args.Length == 1 ? Math.Sin(args[0]) : throw new ArgumentException("Функция 'sin' должна иметь 1 аргумент."),
                "cos" => args.Length == 1 ? Math.Cos(args[0]) : throw new ArgumentException("Функция 'cos' должна иметь 1 аргумент."),
                "tg" => args.Length == 1 ? Math.Tan(args[0]) : throw new ArgumentException("Функция 'tg' должна иметь 1 аргумент."),
                "ctg" => args.Length == 1 ? 1 / Math.Tan(args[0]) : throw new ArgumentException("Функция 'ctg' должна иметь 1 аргумент."),
                _ => throw new ArgumentException($"Неподдерживаемая функция: {function.Name}")
            };
        }

        //Метод для выполнения унарных операций
        private static double ApplyUnaryOperation(char op, double a)
        {
            return op switch
            {
                '-' => -a,
                '+' => a,
                _ => throw new ArgumentException($"Неподдерживаемая унарная операция: {op}")
            };
        }
        
        //Метод для операций между двумя числами на основе переданного оператора
        private static double ApplyOperation(char op, double a, double b)
        {
            return op switch
            {
                '+' => a + b,
                '-' => a - b,
                '*' => a * b,
                '/' => b == 0 ? throw new DivideByZeroException("Деление на ноль.") : a / b,
                '^' => Math.Pow(a, b),
                _ => throw new ArgumentException($"Неподдерживаемая операция: {op}")
            };
        }
    }
}