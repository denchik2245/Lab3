using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WithWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PlotGraphButton_Click(object sender, RoutedEventArgs e)
        {
            double start = double.Parse(StartValueTextBox.Text);
            double end = double.Parse(EndValueTextBox.Text);
            double step = double.Parse(StepTextBox.Text);
            double scale = double.Parse(ScaleTextBox.Text);

            PlotGraph(start, end, step, scale);
        }

        private void PlotGraph(double start, double end, double step, double scale)
        {
            GraphCanvas.Children.Clear();

            Polyline polyline = new Polyline();
            polyline.Stroke = Brushes.Blue;

            for (double x = start; x <= end; x += step)
            {
                double y = CalculateFunction(x);
                Point point = new Point((x - start) * scale, -(y * scale) + GraphCanvas.ActualHeight / 2);
                polyline.Points.Add(point);
            }

            GraphCanvas.Children.Add(polyline);
        }

        private double CalculateFunction(double x)
        {
            // Ваш код для вычисления функции
            return Math.Sin(x);
        }
    }
}