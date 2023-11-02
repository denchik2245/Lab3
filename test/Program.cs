using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LabsForCsu
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите математическое выражение:");
            var input = Console.ReadLine();

            var postfix = ConvertToPostfix(input);
            Console.WriteLine($"Обратная польская запись: {string.Join(" ", postfix)}");

            var result = EvaluatePostfix(postfix);
            Console.WriteLine($"Результат вычисления: {result}");
        }

        // Метод для преобразования инфиксного выражения в постфиксное (ОПЗ).
        static List<string> ConvertToPostfix(string expression)
        {
            List<string> postfix = new List<string>();
            Stack<char> ops = new Stack<char>();

            for (int i = 0; i < expression.Length; i++)
            {
                if (char.IsDigit(expression[i]) || expression[i] == '.')
                {
                    string number = "";
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        number += expression[i++];
                    postfix.Add(number);
                    i--;
                }
                else if (expression[i] == '(')
                {
                    ops.Push(expression[i]);
                }
                else if (expression[i] == ')')
                {
                    while (ops.Count > 0 && ops.Peek() != '(')
                        postfix.Add(ops.Pop().ToString());
                    if (ops.Count > 0 && ops.Peek() == '(')
                        ops.Pop();
                }
                else if ("+-*/".Contains(expression[i]))
                {
                    while (ops.Count != 0 && HasPrecedence(expression[i], ops.Peek()))
                        postfix.Add(ops.Pop().ToString());
                    ops.Push(expression[i]);
                }
            }

            while (ops.Count != 0)
                postfix.Add(ops.Pop().ToString());

            return postfix;
        }

        // Метод для вычисления значения выражения в ОПЗ.
        static double EvaluatePostfix(List<string> postfix)
        {
            Stack<double> values = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    values.Push(val);
                }
                else 
                    values.Push(ApplyOperation(char.Parse(token), values.Pop(), values.Pop()));
            }

            return values.Pop();
        }

        static bool HasPrecedence(char op1, char op2)
        {
            if (op2 == '(' || op2 == ')')
                return false;
            if ((op1 == '*' || op1 == '/') && (op2 == '+' || op2 == '-'))
                return false;
            else
                return true;
        }

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
}