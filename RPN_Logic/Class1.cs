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
            //Создаем список для хранения токенов
            var tokens = new List<Token>();
            //Переменные для текущего числа и текущей переменно
            var currentNum = new StringBuilder();
            var currentVar = new StringBuilder();

            //Проходим по каждому символу во входной строке
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                //Если символ - цифра или точка, добавляем его к текущему числу
                if (char.IsDigit(ch) || ch == '.')
                {
                    currentNum.Append(ch);
                }
                else
                {
                    //Если текущее число не пустое, добавляем его как токен Number
                    if (currentNum.Length > 0)
                    {
                        tokens.Add(new Number(double.Parse(currentNum.ToString(), CultureInfo.InvariantCulture)));
                        currentNum.Clear();
                    }
                    //Если символ - буква, добавляем его к текущей переменной
                    if (char.IsLetter(ch))
                    {
                        currentVar.Append(ch);
                    }
                    else
                    {
                        //Если текущая переменная не пуста, обрабатываем ее как функцию или переменную
                        if (currentVar.Length > 0)
                        {
                            ProcessVariable(currentVar.ToString(), tokens);
                            currentVar.Clear();
                        }
                        //Добавляем операцию или скобку в список токенов
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
            //Добавляем последнее число, если оно есть
            if (currentNum.Length > 0)
            {
                tokens.Add(new Number(double.Parse(currentNum.ToString(), CultureInfo.InvariantCulture)));
            }
            //Обрабатываем последнюю переменную, если она есть
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
                //Пытаемся определить количество аргументов функции
                int argsCount = FunctionArgumentsCount(var);
                //Добавляем функцию с количеством аргументовДобавляем функцию с количеством аргументов
                tokens.Add(new Function(var, argsCount));
            }
            catch (ArgumentException)
            {
                //Если не удалось определить количество аргументов, добавляем переменную
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
            var postfix = new List<Token>(); //Создаем список для хранения токенов в постфиксной форме
            var operationStack = new Stack<Token>(); //Создаем стек для операций

            //Проходим по каждому токену из входного списка
            foreach (var token in tokens)
            {
                switch (token) //Проверяем тип токена
                {
                    case Number number: //Если токен - число
                        postfix.Add(number); //Добавляем число в постфиксную запись
                        break;
                    
                    case UnaryOperation unaryOperation: //Если токен - унарная операция
                        while (operationStack.Count != 0 &&
                               operationStack.Peek() is Operation topOperation &&
                               topOperation.Priority >= unaryOperation.Priority)
                        {
                            postfix.Add(operationStack.Pop()); //Пока стек не пуст и верхний элемент стека имеет больший или равный приоритет, добавляем операции из стека в постфиксную запись
                        }
                        operationStack.Push(unaryOperation); //Добавляем унарную операцию в стек
                        break;
                    
                    case Operation operation: //Если токен - операция
                        while (operationStack.Count != 0 &&
                               operationStack.Peek() is Operation topOperation &&
                               topOperation.Priority >= operation.Priority)
                        {
                            postfix.Add(operationStack.Pop()); //Пока стек не пуст и верхний элемент стека имеет больший или равный приоритет, добавляем операции из стека в постфиксную запись
                        }
                        operationStack.Push(operation); //Добавляем операцию в стек
                        break;
                    
                    case Parenthesis parenthesis when parenthesis.Symbol == '(': //Если токен - открывающая скобка
                        operationStack.Push(parenthesis); //Добавляем скобку в стек
                        break;
                    
                    case Parenthesis parenthesis when parenthesis.Symbol == ')': //Если токен - закрывающая скобка
                        while (operationStack.Count > 0 && !(operationStack.Peek() is Parenthesis p && p.Symbol == '('))
                        {
                            postfix.Add(operationStack.Pop()); //Пока стек не пуст и верхний элемент стека не является открывающей скобкой, добавляем операции из стека в постфиксную запись
                        }
                        if (operationStack.Count > 0 && operationStack.Peek() is Parenthesis)
                        {
                            operationStack.Pop(); //Удаляем открывающую скобку из стека
                        }
                        break;
                    
                    case Function function when function.Name == "sqrt": // Если функция - квадратный корень
                        operationStack.Push(function); // Добавляем функцию в стек
                        break;
                    
                    case Function function when function.Name == "sin":
                        operationStack.Push(function);
                        break;
                    
                    case Function function when function.Name == "cos":
                        operationStack.Push(function);
                        break;
                    
                    case Function function when function.Name == "tg":
                        operationStack.Push(function);
                        break;
                    
                    case Function function when function.Name == "ctg":
                        operationStack.Push(function);
                        break;
                    
                    case Function function when function.Name == "log":
                        operationStack.Push(function);
                        break;
                    
                    case Variable variable: //Если токен - переменная
                        postfix.Add(variable); //Добавляем переменную в постфиксную запись
                        break;
                }
            }

            while (operationStack.Count != 0) //Пока стек не пуст
            {
                var remainingToken = operationStack.Pop();
                if (!(remainingToken is Parenthesis))
                {
                    postfix.Add(remainingToken); //Добавляем оставшиеся операции из стека в постфиксную запись
                }
            }

            return postfix;
        }

        public static double EvaluatePostfix(List<Token> postfix, Dictionary<string, double> variableValues)
        {
            var values = new Stack<double>(); //Создаем стек для хранения промежуточных значений
            double[] args = null; //Массив для аргументов функции

            foreach (var token in postfix) //Проходим по каждому токену в постфиксной записи
            {
                switch (token) //Проверяем тип токена
                {
                    case Number number: //Если токен - число
                        values.Push(number.Value); //Добавляем число в стек
                        break;

                    case UnaryOperation unaryOperation: //Если токен - унарная операция
                        double unaryValue = values.Pop(); //Извлекаем значение из стека
                        values.Push(ApplyUnaryOperation(unaryOperation.Symbol, unaryValue)); //Применяем унарную операцию и добавляем результат в стек
                        break;

                    case Operation operation: //Если токен - операция
                        double b = values.Pop(); //Извлекаем второй операнд
                        double a = values.Pop(); //Извлекаем первый операнд
                        values.Push(ApplyOperation(operation.Symbol, a, b)); //Применяем операцию и добавляем результат в стек
                        break;

                    case Function function: //Если токен - функция
                        args = new double[function.ArgumentsCount]; //Создаем массив для аргументов функции
                        for (int i = function.ArgumentsCount - 1; i >= 0; i--)
                        {
                            args[i] = values.Pop(); //Извлекаем аргументы функции из стека
                        }
                        values.Push(ApplyFunction(function, args)); //Применяем функцию и добавляем результат в стек
                        break;

                    case Variable variable: //Если токен - переменная
                        if (!variableValues.TryGetValue(variable.Name, out double varValue)) //Проверяем, есть ли значение переменной в словаре
                            throw new ArgumentException($"Unknown variable: {variable.Name}"); //Вызываем исключение, если переменная не найдена
                        values.Push(varValue); //Добавляем значение переменной в стек
                        break;
                }
            }

            if (values.Count != 1) //Проверяем, что в стеке осталось только одно значение
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