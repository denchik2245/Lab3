using System.Windows;
using RPN_Logic;

namespace WithWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Обработчик событий кнопки "Вычислить"
        private void CalculateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(input))
                {
                    ResultLabel.Content = "Введите выражение.";
                    return;
                }

                double variableValue = 0;
                var variableValueText = VariableValueTextBox.Text.Trim();
                bool isVariableRequired = input.Contains("x");

                if (isVariableRequired && (string.IsNullOrWhiteSpace(variableValueText) || !double.TryParse(variableValueText, out variableValue)))
                {
                    ResultLabel.Content = "Некорректное значение переменной x.";
                    return;
                }

                var tokens = Calculator.Tokenize(input);
                var postfix = Calculator.ConvertToPostfix(tokens);
                var variables = isVariableRequired ? new Dictionary<string, double> { { "x", variableValue } } : new Dictionary<string, double>();
                var result = Calculator.EvaluatePostfix(postfix, variables);

                ResultLabel.Content = $"Результат: {result}";
            }
            catch (Exception ex)
            {
                ResultLabel.Content = $"Ошибка: {ex.Message}";
            }
        }
    }
}   


