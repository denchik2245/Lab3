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
        private const string InputTextBoxPlaceholder = "Введите выражение";
        private const string StartRangePlaceholder = "Начало";
        private const string EndRangePlaceholder = "Конец";
        private const string StepPlaceholder = "Шаг";
        private const string ScalePlaceholder = "Масштаб";

        public MainWindow()
        {
            InitializeComponent();
            InputTextBox.Text = InputTextBoxPlaceholder;
            StartRangeTextBox.Text = StartRangePlaceholder;
            EndRangeTextBox.Text = EndRangePlaceholder;
            StepTextBox.Text = StepPlaceholder;
            ScaleTextBox.Text = ScalePlaceholder;
        }
        
        private void StartRangeTextBox_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(StartRangeTextBox, StartRangePlaceholder);
        private void EndRangeTextBox_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(EndRangeTextBox, EndRangePlaceholder);
        private void StepTextBox_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(StepTextBox, StepPlaceholder);
        private void ScaleTextBox_GotFocus(object sender, RoutedEventArgs e) => ClearPlaceholder(ScaleTextBox, ScalePlaceholder);
        
        private void StartRangeTextBox_LostFocus(object sender, RoutedEventArgs e) => SetPlaceholder(StartRangeTextBox, StartRangePlaceholder);
        private void EndRangeTextBox_LostFocus(object sender, RoutedEventArgs e) => SetPlaceholder(EndRangeTextBox, EndRangePlaceholder);
        private void StepTextBox_LostFocus(object sender, RoutedEventArgs e) => SetPlaceholder(StepTextBox, StepPlaceholder);
        private void ScaleTextBox_LostFocus(object sender, RoutedEventArgs e) => SetPlaceholder(ScaleTextBox, ScalePlaceholder);

        private void ClearPlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox.Text == placeholder)
            {
                textBox.Text = string.Empty;
            }
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
            }
        }

        private void BuildGraphButtonClick(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(StartRangeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double startRange) ||
                !double.TryParse(EndRangeTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double endRange) ||
                !double.TryParse(StepTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double step) ||
                !double.TryParse(ScaleTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double scale) ||
                InputTextBox.Text == InputTextBoxPlaceholder)
            {
                MessageBox.Show("Все поля должны быть заполнены корректными значениями.");
                return;
            }

            DrawGraph(InputTextBox.Text, startRange, endRange, step, scale);
        }

        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == InputTextBoxPlaceholder)
            {
                InputTextBox.Text = string.Empty;
            }
        }

        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                InputTextBox.Text = InputTextBoxPlaceholder;
            }
        }
        private void DrawGraph(string input, double startRange, double endRange, double step, double scale)
        {
            GraphCanvas.Children.Clear();
            
            double centerCanvasX = GraphCanvas.ActualWidth / 2;
            double centerCanvasY = GraphCanvas.ActualHeight / 2;
            
            DrawGrid(centerCanvasX, centerCanvasY, step, scale);
            
            Polyline graphLine = new Polyline
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            for (double x = startRange; x <= endRange; x += step)
            {
                string expression = input.Replace("x", x.ToString(CultureInfo.InvariantCulture));
                
                var tokens = Calculator.Tokenize(expression);
                var postfix = Calculator.ConvertToPostfix(tokens);
                double result = Calculator.EvaluatePostfix(postfix, new Dictionary<string, double> { { "x", x } });
                
                Point graphPoint = new Point(centerCanvasX + (x * scale), centerCanvasY - (result * scale));
                graphLine.Points.Add(graphPoint);
            }
            
            GraphCanvas.Children.Add(graphLine);
        }


        private void DrawGrid(double centerX, double centerY, double step, double scale)
        {
            Line axisX = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = GraphCanvas.ActualWidth,
                Y2 = centerY,
                Stroke = Brushes.LightGray
            };

            Line axisY = new Line
            {
                X1 = centerX,
                Y1 = 0,
                X2 = centerX,
                Y2 = GraphCanvas.ActualHeight,
                Stroke = Brushes.LightGray
            };

            GraphCanvas.Children.Add(axisX);
            GraphCanvas.Children.Add(axisY);
            
            for (double i = step * scale; i < GraphCanvas.ActualWidth / 2; i += step * scale)
            {
                GraphCanvas.Children.Add(CreateTick(centerX + i, centerY));
                GraphCanvas.Children.Add(CreateTick(centerX - i, centerY));
            }

            for (double i = step * scale; i < GraphCanvas.ActualHeight / 2; i += step * scale)
            {
                GraphCanvas.Children.Add(CreateTick(centerX, centerY + i));
                GraphCanvas.Children.Add(CreateTick(centerX, centerY - i));
            }
        }

        private Line CreateTick(double x, double y)
        {
            return new Line
            {
                X1 = x,
                Y1 = y - 5,
                X2 = x,
                Y2 = y + 5,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
        }
    }
}
