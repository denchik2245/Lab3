using System; // Импортирование основного пространства имен .NET
using System.Collections.Generic; // Импортирование пространства имен, содержащего типы коллекций, такие как List и Dictionary.

public class Program // Объявление основного класса программы
{
    public static void Main() // Главная точка входа в программу
    {
        Console.WriteLine("Введите математическое выражение:"); // Вывод приветственного сообщения
        string expression = Console.ReadLine(); // Чтение введенного пользователем выражения
        
        // 1 лабораторная
        List<double> numbers = new List<double>(); // Список для хранения чисел из выражения
        List<Tuple<char, int>> operations = new List<Tuple<char, int>>(); // Список для хранения операций и их приоритетов
        ParseExpression(expression, numbers, operations); // Разбор выражения на числа и операции
        
        Console.WriteLine("Числа:"); // Вывод заголовка списка чисел
        foreach (double num in numbers) // Цикл по списку чисел
        {
            Console.Write(num + " "); // Вывод каждого числа с пробелом
        }
        
        Console.WriteLine("\nОперации:"); // Вывод заголовка списка операций
        foreach (var operation in operations) // Цикл по списку операций
        {
            Console.Write(operation.Item1 + " "); // Вывод символа операции
        }

        // 2 и 3 лабораторные
        double result = EvaluateExpression(numbers, operations); // Вычисление результата выражения
        Console.WriteLine("\nРезультат: " + result); // Вывод результата
    }

    // Метод разбора выражения на числа и операции
    public static void ParseExpression(string expression, List<double> numbers, List<Tuple<char, int>> operations)
    {
        int priority = 0; // Начальный приоритет операций

        string number = ""; // Текущая считываемая строка числа
        foreach (char c in expression) // Цикл по каждому символу в выражении
        {
            if (char.IsDigit(c) || c == '.') // Если символ является цифрой или точкой
            {
                number += c; // Добавляем символ к текущей строке числа
            }
            else if (!char.IsWhiteSpace(c)) // Если символ не является пробелом
            {
                if (number.Length > 0) // Если строка числа не пуста
                {
                    numbers.Add(Convert.ToDouble(number)); // Преобразуем строку в число и добавляем в список
                    number = ""; // Очищаем строку числа для следующего числа
                }

                switch (c) // Определение операции и ее приоритета
                {
                    case '+':
                    case '-':
                        operations.Add(new Tuple<char, int>(c, priority));
                        break;
                    case '*':
                    case '/':
                        operations.Add(new Tuple<char, int>(c, priority + 1));
                        break;
                    case '(':
                        priority += 1;
                        break;
                    case ')':
                        priority -= 1;
                        break;
                }
            }
        }
        
        if (number.Length > 0) // Если после окончания цикла осталось необработанное число
        {
            numbers.Add(Convert.ToDouble(number)); // Преобразуем его в число и добавляем в список
        }
    }
    
    // Метод вычисления результата выражения
    public static double EvaluateExpression(List<double> numbers, List<Tuple<char, int>> operations)
    {
        while (operations.Count > 0) // Пока есть операции для обработки
        {
            int maxPriorityIndex = 0; // Индекс операции с максимальным приоритетом
            int maxPriority = operations[0].Item2; // Значение максимального приоритета

            for (int i = 1; i < operations.Count; i++) // Цикл по операциям для нахождения операции с максимальным приоритетом
            {
                if (operations[i].Item2 > maxPriority)
                {
                    maxPriority = operations[i].Item2;
                    maxPriorityIndex = i;
                }
            }

            double left = numbers[maxPriorityIndex]; // Левый операнд
            double right = numbers[maxPriorityIndex + 1]; // Правый операнд
            char operation = operations[maxPriorityIndex].Item1; // Операция

            numbers.RemoveAt(maxPriorityIndex + 1); // Удаление правого операнда из списка чисел
            numbers[maxPriorityIndex] = PerformOperation(left, right, operation); // Вычисление результата и замена левого операнда этим результатом
            operations.RemoveAt(maxPriorityIndex); // Удаление выполненной операции из списка
        }

        return numbers[0]; // Возврат результата
    }

    // Метод выполнения операции над двумя числами
    public static double PerformOperation(double left, double right, char operation)
    {
        switch (operation) // Определение операции
        {
            case '+': return left + right;
            case '-': return left - right;
            case '*': return left * right;
            case '/': return left / right;
            default: throw new ArgumentException($"Неподдерживаемая операция: {operation}"); // Ошибка для неизвестной операции
        }
    }
}