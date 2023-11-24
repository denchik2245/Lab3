using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LabsForCsu
{
    class Program
    {
        // Главная точка входа в программу.
        static void Main(string[] args)
        {
            // Запрос у пользователя ввести математическое выражение.
            Console.WriteLine("Введите математическое выражение:");
            var input = Console.ReadLine();

            // Токенизация (разбиение на числа и операции) введенного выражения.
            var tokens = Tokenize(input);
            
            // Вывод списка чисел.
            Console.WriteLine("\nЧисла:");
            Console.WriteLine(string.Join(" ", tokens.Item1));
            
            // Вывод списка операций.
            Console.WriteLine("\nОперации:");
            Console.WriteLine(string.Join(" ", tokens.Item2.Select(o => o.Operation)));

            // Вычисление и вывод результата выражения.
            var result = EvaluateExpression(input);
            Console.WriteLine($"\nРезультат вычисления: {result}");
        }

        // Метод для разбиения строки на числа и операции с приоритетами.
        static (List<double>, List<OperationPriority>) Tokenize(string input)
        {
            var numbers = new List<double>(); // Список для хранения чисел.
            var operations = new List<OperationPriority>(); // Список для хранения операций с их приоритетами.
            var currentNum = ""; // Текущее считываемое число.
            int currentPriority = 0; // Текущий приоритет операций (увеличивается или уменьшается при встрече скобок).

            // Перебор каждого символа в строке.
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i]; // Записываем символ
                
                if (char.IsDigit(ch) || ch == '.') // Если символ является числом или точкой.
                {
                    currentNum += ch; // Добавляем в текущее число
                }
                else
                {
                    // Если завершено считывание числа.
                    if (!string.IsNullOrWhiteSpace(currentNum))
                    {
                        // Добавление числа в список и очистка текущего числа.
                        numbers.Add(double.Parse(currentNum, CultureInfo.InvariantCulture));
                        currentNum = "";
                    }

                    // Обработка скобок и операций.
                    if (ch == '(')
                    {
                        currentPriority++;
                    }
                    else if (ch == ')')
                    {
                        currentPriority--;
                    }
                    else if ("+-*/".Contains(ch))
                    {
                        // Определение приоритета операции.
                        int priority = (ch == '*' || ch == '/') ? currentPriority + 1 : currentPriority;
                        operations.Add(new OperationPriority { Operation = ch, Priority = priority });
                    }
                }
            }

            // Если после завершения строки осталось число.
            if (!string.IsNullOrWhiteSpace(currentNum))
            {
                numbers.Add(double.Parse(currentNum, CultureInfo.InvariantCulture));
            }

            return (numbers, operations);
        }

        // Метод для вычисления значения выражения.
        static double EvaluateExpression(string expression)
        {
            // Стек для хранения чисел.
            Stack<double> values = new Stack<double>();
            // Стек для хранения операций.
            Stack<char> ops = new Stack<char>();

            // Перебор каждого символа в строке выражения.
            for (int i = 0; i < expression.Length; i++)
            {
                // Пропуск пробелов.
                if (expression[i] == ' ')
                    continue;

                // Обработка скобки.
                if (expression[i] == '(')
                {
                    ops.Push(expression[i]);
                }
                // Обработка чисел.
                else if (char.IsDigit(expression[i]))
                {
                    string sVal = "";
                    // Считывание полного числа.
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        sVal += expression[i++];

                    values.Push(double.Parse(sVal, CultureInfo.InvariantCulture));
                    i--;
                }
                // Обработка закрытия скобки.
                else if (expression[i] == ')')
                {
                    // Применение операций до открывающей скобки.
                    while (ops.Peek() != '(')
                        values.Push(ApplyOperation(ops.Pop(), values.Pop(), values.Pop()));
                    ops.Pop();
                }
                // Обработка арифметических операций.
                else if ("+-*/".Contains(expression[i]))
                {
                    // Применение предыдущих операций с более высоким приоритетом.
                    while (ops.Count != 0 && HasPrecedence(expression[i], ops.Peek()))
                        values.Push(ApplyOperation(ops.Pop(), values.Pop(), values.Pop()));

                    ops.Push(expression[i]);
                }
            }

            // Применение оставшихся операций.
            while (ops.Count != 0)
                values.Push(ApplyOperation(ops.Pop(), values.Pop(), values.Pop()));

            return values.Pop();
        }

        // Проверка приоритета операций.
        static bool HasPrecedence(char op1, char op2)
        {
            if (op2 == '(' || op2 == ')')
                return false;
            if ((op1 == '*' || op1 == '/') && (op2 == '+' || op2 == '-'))
                return false;
            else
                return true;
        }

        // Применение операции к двум числам.
        static double ApplyOperation(char op, double b, double a)
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
            }
            return 0;
        }
    }

    // Структура для хранения операции и ее приоритета.
    struct OperationPriority
    {
        public char Operation { get; set; }
        public int Priority { get; set; }
    }
}