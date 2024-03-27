using RPN_Logic;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите математическое выражение:");
            var input = Console.ReadLine();
            var tokens = Calculator.Tokenize(input);

            Console.WriteLine("\nОбратная польская запись: ");
            var postfix = Calculator.ConvertToPostfix(tokens);
            foreach (var token in postfix)
            {
                if (token is Number number)
                {
                    Console.Write($"{number.Value} ");
                }
                else if (token is Operation operation)
                {
                    Console.Write($"{operation.Symbol} ");
                }
                // Для других типов токенов можно добавить дополнительные условия, если необходимо.
            }

            Console.WriteLine("\n\nРезультат вычисления: ");
            Console.WriteLine(Calculator.EvaluatePostfix(postfix));
        }

    }
}