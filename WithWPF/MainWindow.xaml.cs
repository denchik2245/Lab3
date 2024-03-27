using System.Windows;
using RPN_Logic;

namespace WithWPF
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = InputTextBox.Text;
                var tokens = Calculator.Tokenize(input);
                var postfix = Calculator.ConvertToPostfix(tokens);
                var result = Calculator.EvaluatePostfix(postfix);
                ResultLabel.Content = result.ToString();
            }
            catch (Exception ex)
            {
                ResultLabel.Content = $"Ошибка: {ex.Message}";
            }
        }

    }
}



