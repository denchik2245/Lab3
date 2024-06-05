using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RPN_Logic;

namespace WithWPF
{
    public partial class MainWindow : Window
    {
        private double _scaleFactor = 10;

        public MainWindow()
        {
            InitializeComponent();
            GraphScrollViewer.PreviewMouseWheel += GraphScrollViewer_PreviewMouseWheel;
            GraphCanvas.PreviewMouseWheel += GraphCanvas_PreviewMouseWheel;
            DrawAxes();
            BuildGraph("");
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GraphScrollViewer.ScrollToHorizontalOffset((GraphCanvas.Width - GraphScrollViewer.ViewportWidth) / 2);
            GraphScrollViewer.ScrollToVerticalOffset((GraphCanvas.Height - GraphScrollViewer.ViewportHeight) / 2);
        }
        
        private void BuildGraph(string expression)
        {
            GraphCanvas.Children.Clear();
            DrawAxes();

            if (string.IsNullOrWhiteSpace(expression))
            {
                return;
            }

            double minX = -20, maxX = 20;
            double step = 1 / (_scaleFactor * ScaleSlider.Value / 100);
            double scale = _scaleFactor * ScaleSlider.Value / 100;
            DrawGraph(expression, minX, maxX, step, scale);
        }

        private void DrawAxes()
        {
            GraphCanvas.Children.Clear();

            double scale = _scaleFactor * ScaleSlider.Value / 100;
            double centerX = GraphCanvas.Width / 2;
            double centerY = GraphCanvas.Height / 2;

            // Горизонтальная ось (ось X)
            Line xAxis = new Line
            {
                X1 = 0,
                X2 = GraphCanvas.Width,
                Y1 = centerY,
                Y2 = centerY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(xAxis);

            // Вертикальная ось (ось Y)
            Line yAxis = new Line
            {
                X1 = centerX,
                X2 = centerX,
                Y1 = 0,
                Y2 = GraphCanvas.Height,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(yAxis);

            // Стрелка на оси X
            Polygon xArrow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(GraphCanvas.Width - 10, centerY - 5),
                    new Point(GraphCanvas.Width - 10, centerY + 5),
                    new Point(GraphCanvas.Width, centerY)
                },
                Fill = Brushes.Black
            };
            GraphCanvas.Children.Add(xArrow);

            // Стрелка на оси Y
            Polygon yArrow = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(centerX - 5, 10),
                    new Point(centerX + 5, 10),
                    new Point(centerX, 0)
                },
                Fill = Brushes.Black
            };
            GraphCanvas.Children.Add(yArrow);

            // Штрихи и подписи на оси X
            int tickStep = Math.Max(1, (int)Math.Ceiling(100 / scale));
            int startX = (int)(-GraphCanvas.Width / 2 / scale);
            int endX = (int)(GraphCanvas.Width / 2 / scale);
            for (int x = startX; x <= endX; x += tickStep)
            {
                double canvasX = x * scale + centerX;

                if (canvasX >= 0 && canvasX <= GraphCanvas.Width)
                {
                    Line tick = new Line
                    {
                        X1 = canvasX,
                        X2 = canvasX,
                        Y1 = centerY - 5,
                        Y2 = centerY + 5,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    GraphCanvas.Children.Add(tick);

                    if (x != 0)
                    {
                        TextBlock label = new TextBlock
                        {
                            Text = x.ToString(),
                            Foreground = Brushes.Black,
                            FontSize = 10
                        };
                        Canvas.SetLeft(label, canvasX - label.ActualWidth / 2);
                        Canvas.SetTop(label, centerY + 10);
                        GraphCanvas.Children.Add(label);
                    }
                }
            }

            // Штрихи и подписи на оси Y
            int startY = (int)(-GraphCanvas.Height / 2 / scale);
            int endY = (int)(GraphCanvas.Height / 2 / scale);
            for (int y = startY; y <= endY; y += tickStep)
            {
                double canvasY = centerY - y * scale;

                if (canvasY >= 0 && canvasY <= GraphCanvas.Height)
                {
                    Line tick = new Line
                    {
                        X1 = centerX - 5,
                        X2 = centerX + 5,
                        Y1 = canvasY,
                        Y2 = canvasY,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    GraphCanvas.Children.Add(tick);

                    if (y != 0)
                    {
                        TextBlock label = new TextBlock
                        {
                            Text = y.ToString(),
                            Foreground = Brushes.Black,
                            FontSize = 10
                        };
                        Canvas.SetLeft(label, centerX + 10);
                        Canvas.SetTop(label, canvasY - label.ActualHeight / 2);
                        GraphCanvas.Children.Add(label);
                    }
                }
            }
        }
        
        private void DrawGraph(string expression, double minX, double maxX, double step, double scale)
        {
            Polyline graphLine = new Polyline
            {
                Stroke = Brushes.OrangeRed,
                StrokeThickness = 2
            };

            for (double x = minX; x <= maxX; x += step)
            {
                try
                {
                    var tokens = Calculator.Tokenize(expression);
                    var postfix = Calculator.ConvertToPostfix(tokens);
                    double result = Calculator.EvaluatePostfix(postfix, new System.Collections.Generic.Dictionary<string, double> { { "x", x } });

                    double canvasX = x * scale + GraphCanvas.Width / 2;
                    double canvasY = GraphCanvas.Height / 2 - result * scale;

                    if (canvasX >= 0 && canvasX <= GraphCanvas.Width && canvasY >= 0 && canvasY <= GraphCanvas.Height)
                    {
                        Point graphPoint = new Point(canvasX, canvasY);
                        graphLine.Points.Add(graphPoint);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при вычислении графика: {ex.Message}");
                }
            }

            GraphCanvas.Children.Add(graphLine);
        }
        
        private void GraphCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double deltaScale = e.Delta > 0 ? 1.1 : 0.9;
            ScaleSlider.Value = Math.Max(ScaleSlider.Minimum, Math.Min(ScaleSlider.Maximum, ScaleSlider.Value * deltaScale));
            e.Handled = true;
        }


        private void BuildGraphButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                MessageBox.Show("Введите выражение для построения графика.");
                return;
            }

            BuildGraph(InputTextBox.Text);
        }

        private void GraphScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
        
        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                BuildGraph(InputTextBox.Text);
            }
            else
            {
                DrawAxes();
            }
        }
    }
}