using System.Globalization;
using System.Text;

namespace LabsForCsu
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите математическое выражение:");
            var input = Console.ReadLine();
            
            Console.WriteLine("\nОбратная польская запись: "); // Вывод ОПЗ
            Console.WriteLine(string.Join(" ", ConvertToPostfix(input)));
            
            Console.WriteLine("\nРезультат вычисления: "); // Вывод значения выражения
            Console.WriteLine(EvaluatePostfix(ConvertToPostfix(input)));
        }
        
        // Метод для преобразования в ОПЗ
        static List<object> ConvertToPostfix(string expression)
        {
            List<object> finalExpr = new List<object>();
            Stack<char> operation = new Stack<char>();

            for (int i = 0; i < expression.Length; i++)
            {
                if (char.IsDigit(expression[i]) || expression[i] == '.')
                {
                    var number = new StringBuilder();
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        number.Append(expression[i++]);
                    finalExpr.Add(number.ToString());
                    i--;
                }
                else if (expression[i] == '(')
                {
                    operation.Push(expression[i]);
                }
                else if ("+-*/".Contains(expression[i]))
                {
                    while (operation.Count != 0 && Priority(operation.Peek()) >= Priority(expression[i]))
                        finalExpr.Add(operation.Pop());
                    operation.Push(expression[i]);
                }
                else if (expression[i] == ')')
                {
                    while (operation.Count > 0 && operation.Peek() != '(')
                        finalExpr.Add(operation.Pop());
                    if (operation.Count > 0 && operation.Peek() == '(')
                        operation.Pop();
                }
            }

            while (operation.Count != 0)
                finalExpr.Add(operation.Pop());

            return finalExpr;
        }
        
        static double EvaluatePostfix(List<object> postfix)
        {
            Stack<double> values = new Stack<double>();

            foreach (var token in postfix)
            {
                if (token is string str && double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    values.Push(val);
                }
                else if (token is char op)
                {
                    if (values.Count < 2)
                        throw new InvalidOperationException("Недостаточно данных в стеке для выполнения операции");
                    values.Push(ApplyOperation(op, values.Pop(), values.Pop()));
                }
                else
                {
                    throw new InvalidOperationException("Некорректный элемент в выражении: " + token);
                }
            }

            return values.Pop();
        }
        
        // Метод для приоритетов
        static int Priority(char op)
        {
            if (op == '*' || op == '/') return 2;
            if (op == '+' || op == '-') return 1;
            return 0;
        }

        // Метод для вычисления значения 2 переменных
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