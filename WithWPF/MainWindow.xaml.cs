using System.Windows;
using RPN_Logic;

namespace WithWPF
{
    public partial class MainWindow : Window
    {
        private const string InputTextBoxPlaceholder = "Введите выражение";
        private const string VariableValueTextBoxPlaceholder = "Значение переменной x";

        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Text = InputTextBoxPlaceholder;
            VariableValueTextBox.Text = VariableValueTextBoxPlaceholder;
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == InputTextBoxPlaceholder)
            {
                InputTextBox.Text = "";
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputTextBox.Text))
            {
                InputTextBox.Text = InputTextBoxPlaceholder;
            }
        }

        private void VariableValueTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (VariableValueTextBox.Text == VariableValueTextBoxPlaceholder)
            {
                VariableValueTextBox.Text = "";
            }
        }

        private void VariableValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(VariableValueTextBox.Text))
            {
                VariableValueTextBox.Text = VariableValueTextBoxPlaceholder;
            }
        }

        private void CalculateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputTextBox.Text.Trim();
                if (input == InputTextBoxPlaceholder || string.IsNullOrWhiteSpace(input))
                {
                    ResultLabel.Content = "Введите выражение.";
                    return;
                }
                
                double variableValue = 0;
                var variableValueText = VariableValueTextBox.Text.Trim();
                bool isVariableRequired = input.Contains("x");

                if (isVariableRequired && (variableValueText == VariableValueTextBoxPlaceholder || !double.TryParse(variableValueText, out variableValue)))
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




