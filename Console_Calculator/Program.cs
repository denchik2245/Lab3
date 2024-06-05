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
                else if (token is Variable variable)
                {
                    Console.Write($"{variable.Name} ");
                }
            }
            
            Console.WriteLine("\n\nВведите значение переменной x:");
            double xValue = double.Parse(Console.ReadLine());
            
            var variableValues = new Dictionary<string, double> { { "x", xValue } };
            
            try
            {
                double result = Calculator.EvaluatePostfix(postfix, variableValues);
                Console.WriteLine("\nРезультат вычисления: ");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nОшибка: " + ex.Message);
            }
        }
    }
}