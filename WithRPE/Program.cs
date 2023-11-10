using System.Globalization;

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
            
            Console.WriteLine("Числа:"); // Вывод списка чисел
            Console.WriteLine(string.Join(" ", Tokenize(input).Item1));
            
            Console.WriteLine("Операции:"); // Вывод списка операций
            Console.WriteLine(string.Join(" ", Tokenize(input).Item2));
            
            Console.WriteLine($"\nРезультат вычисления: "); // Вывод значения выражения
            Console.WriteLine(EvaluatePostfix(ConvertToPostfix(input)));
        }

        // Метод для вывода чисел и операций
        static (List<double>, List<char>) Tokenize(string input)
        {
            var numbers = new List<double>(); // Список для хранения чисел.
            var operations = new List<char>(); // Список для хранения операций с их приоритетами.
            var currentNum = ""; // Текущее считываемое число.

            // Перебор каждого символа в строке.
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i]; // Записываем символ
                
                if (char.IsDigit(ch) || ch == '.') // Если символ является числом или точкой.
                    currentNum += ch; // Добавляем в текущее число
                
                else
                {
                    // Если наше число не пустое
                    if (!string.IsNullOrWhiteSpace(currentNum))
                    {
                        // Добавление числа в список и очистка текущего числа.
                        numbers.Add(double.Parse(currentNum, CultureInfo.InvariantCulture));
                        currentNum = "";
                    }
                    
                    else if ("+-*/".Contains(ch))
                        operations.Add(ch);
                }
            }
            
            if (!string.IsNullOrWhiteSpace(currentNum)) // Если после завершения строки осталось число
                numbers.Add(double.Parse(currentNum, CultureInfo.InvariantCulture));

            return (numbers, operations);
        }
        
        // Метод для преобразования в ОПЗ
        static List<string> ConvertToPostfix(string expression)
        {
            List<string> FinalExpr = new List<string>(); // Лист для хранения финального выражения ОПЗ
            Stack<char> operation = new Stack<char>(); // Стек для хранения операций

            for (int i = 0; i < expression.Length; i++) //Перебираем все символы в исходном выражении
            {
                if (char.IsDigit(expression[i]) || expression[i] == '.') // Если символ равен числу или точке
                {
                    string number = ""; //временная переменная
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.')) // добавляем символы до тех пор, пока не встретим нецифровой символ
                        number += expression[i++]; // добавляем символ к числу и переходим на след. символ
                    FinalExpr.Add(number);
                    i--; // после завершения цикла while, индекс i будет на одну позицию впереди последнего символа числа
                }
                
                if (expression[i] == '(')
                    operation.Push(expression[i]); //помещаем скобку в стек
                
                else if ("+-*/".Contains(expression[i]))
                {
                    while (operation.Count != 0 && HasPrecedence(expression[i], operation.Peek())) // Если оператор на вершине стека имеет больший или равный приоритет, он извлекается из стека и добавляется в финальное выражение
                        FinalExpr.Add(operation.Pop().ToString());
                    operation.Push(expression[i]);
                }
                
                else if (expression[i] == ')') 
                {
                    while (operation.Count > 0 && operation.Peek() != '(')
                        FinalExpr.Add(operation.Pop().ToString()); // операторы извлекаются из стека и добавляются в список, пока не будет найдена открывающая скобка
                    if (operation.Count > 0 && operation.Peek() == '(') // которая удаляется из стека
                        operation.Pop();
                }
            }

            while (operation.Count != 0)
                FinalExpr.Add(operation.Pop().ToString());

            return FinalExpr;
        }

        // Метод для вычисления значения выражения в ОПЗ
        static double EvaluatePostfix(List<string> postfix)
        {
            Stack<double> values = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                    values.Push(val);
                
                else 
                    values.Push(ApplyOperation(char.Parse(token), values.Pop(), values.Pop()));
            }
            
            return values.Pop();
        }

        // Метод для приоритетов
        static bool HasPrecedence(char op1, char op2)
        {
            if (op2 == '(' || op2 == ')')
                return false;
            if ((op1 == '*' || op1 == '/') && (op2 == '+' || op2 == '-'))
                return false;
            else
                return true;
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