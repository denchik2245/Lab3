using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RPN_Logic;

namespace WithWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartRangeTextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void StartRangeTextBox_LostFocus(object sender, RoutedEventArgs e) { }
        private void EndRangeTextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void EndRangeTextBox_LostFocus(object sender, RoutedEventArgs e) { }
        private void StepTextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void StepTextBox_LostFocus(object sender, RoutedEventArgs e) { }
        private void ScaleTextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void ScaleTextBox_LostFocus(object sender, RoutedEventArgs e) { }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e) { }
        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e) { }

        private void BuildGraphButtonClick(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(StartRangeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double startRange) ||
                !double.TryParse(EndRangeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double endRange) ||
                !double.TryParse(StepTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double step) ||
                !double.TryParse(ScaleTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double scale) ||
                string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены корректными значениями.");
                return;
            }

            DrawGraph(InputTextBox.Text, startRange, endRange, step, scale);
        }

        private void DrawGraph(string input, double startRange, double endRange, double step, double scale)
{
    GraphCanvas.Children.Clear();
    GraphBorder.Width = GraphCanvas.ActualWidth;
    GraphBorder.Height = GraphCanvas.ActualHeight;

    double centerCanvasX = GraphBorder.ActualWidth / 2;
    double centerCanvasY = GraphBorder.ActualHeight / 2;

    DrawGrid(centerCanvasX, centerCanvasY, step, scale);

    Polyline graphLine = new Polyline
    {
        Stroke = Brushes.Red,
        StrokeThickness = 2
    };

    double subStep = step / 10;

    for (double x = startRange; x <= endRange; x += subStep)
    {
        try
        {
            var tokens = Calculator.Tokenize(input);
            var postfix = Calculator.ConvertToPostfix(tokens);
            double result = Calculator.EvaluatePostfix(postfix, new Dictionary<string, double> { { "x", x } });

            Point graphPoint = new Point(centerCanvasX + (x * scale), centerCanvasY - (result * scale));
            graphLine.Points.Add(graphPoint);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при вычислении графика: {ex.Message}");
        }
    }

    GraphCanvas.Children.Add(graphLine);
}
        
        private void DrawGrid(double centerX, double centerY, double step, double scale)
        {
            GraphCanvas.Children.Clear();

            Line axisX = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = GraphCanvas.ActualWidth,
                Y2 = centerY,
                Stroke = Brushes.Black
            };

            Line axisY = new Line
            {
                X1 = centerX,
                Y1 = 0,
                X2 = centerX,
                Y2 = GraphCanvas.ActualHeight,
                Stroke = Brushes.Black
            };

            GraphCanvas.Children.Add(axisX);
            GraphCanvas.Children.Add(axisY);

            // Штрихи и числа по оси X
            for (double i = step * scale; i < GraphCanvas.ActualWidth / 2; i += step * scale)
            {
                Line tickXPositive = CreateTick(centerX + i, centerY);
                GraphCanvas.Children.Add(tickXPositive);

                TextBlock labelXPositive = new TextBlock();
                labelXPositive.Text = (i / scale).ToString();
                labelXPositive.Margin = new Thickness(centerX + i - 5, centerY + 5, 0, 0);
                GraphCanvas.Children.Add(labelXPositive);

                Line tickXNegative = CreateTick(centerX - i, centerY);
                GraphCanvas.Children.Add(tickXNegative);

                TextBlock labelXNegative = new TextBlock();
                labelXNegative.Text = (-i / scale).ToString();
                labelXNegative.Margin = new Thickness(centerX - i - 10, centerY + 5, 0, 0);
                GraphCanvas.Children.Add(labelXNegative);
            }

            // Штрихи и числа по оси Y
            for (double i = step * scale; i < GraphCanvas.ActualHeight / 2; i += step * scale)
            {
                Line tickYPositive = CreateTick(centerX, centerY + i);
                GraphCanvas.Children.Add(tickYPositive);

                TextBlock labelYPositive = new TextBlock();
                labelYPositive.Text = (-i / scale).ToString();
                labelYPositive.Margin = new Thickness(centerX - 20, centerY + i - 5, 0, 0);
                GraphCanvas.Children.Add(labelYPositive);

                Line tickYNegative = CreateTick(centerX, centerY - i);
                GraphCanvas.Children.Add(tickYNegative);

                TextBlock labelYNegative = new TextBlock();
                labelYNegative.Text = (i / scale).ToString();
                labelYNegative.Margin = new Thickness(centerX - 20, centerY - i - 15, 0, 0);
                GraphCanvas.Children.Add(labelYNegative);
            }
        }

        private Line CreateTick(double x, double y)
        {
            return new Line
            {
                X1 = x - 5,
                Y1 = y,
                X2 = x + 5,
                Y2 = y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
        }
    }
}
