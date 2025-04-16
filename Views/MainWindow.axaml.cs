using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System.Linq;

namespace GoodbyeDiplom.Views
{
    public partial class MainWindow : Window
    {
        private double GridSize = 5;
        private const int CubeSize = 8;
        private double _cellSize = 40;
        private Point _lastMousePosition;
        private double _angleX = 35 * Math.PI / 180;
        private double _angleY = -45 * Math.PI / 180;
        private bool _isDragging;
        //private double _zoom = 1.0;
        private List<Shape> _surfaceTriangles = new List<Shape>();
        public MainWindow()
        {
            Title = "3D Function Surface (Z Vertical)";
            Width = 800;
            Height = 600;
            
            this.Content = new Canvas();
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;
            this.PointerWheelChanged += OnPointerWheelChanged;
            this.SizeChanged += OnSizeChanged;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            UpdateGrid();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGrid();
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            _lastMousePosition = e.GetPosition(this);
            _isDragging = true;
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isDragging) return;
            
            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _lastMousePosition;
            
            _angleY -= delta.X * 0.01;
            _angleX += delta.Y * 0.01;
            _angleX = Math.Clamp(_angleX, -Math.PI/2 + 0.1, Math.PI/2 - 0.1);
            
            _lastMousePosition = currentPosition;
            UpdateGrid();
        }

        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            // Инвертированное масштабирование (скролл вперёд - приближение)
            var zoomFactor = e.Delta.Y > 0 ? 0.9 : 1.1;
            GridSize *= zoomFactor;
            // _zoom = Math.Clamp(_zoom, 0.2, 5.0);
            GridSize = Math.Clamp(GridSize, 0.01, 50000);
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            if (Content is not Canvas canvas) return;

            canvas.Children.Clear();
            _surfaceTriangles.Clear();
            
            double centerX = Bounds.Width / 2;
            double centerY = Bounds.Height / 2;

            // Фиксированный размер для куба, осей и СЕТКИ
            double fixedCellSize = Math.Min(Bounds.Width, Bounds.Height) / 20;
            
            // Масштабируемый размер только для функции и числовых подписей
            //double gridCellSize = fixedCellSize / _zoom;

            // Адаптивный шаг сетки (увеличивается при уменьшении масштаба)
            int dynamicGridStep = (int)Math.Max(1, 4);
            
            // 1. Рисуем кубический каркас (статично)
            DrawCubeFrame(canvas, centerX, centerY, fixedCellSize);
            
            // 2. Рисуем координатные плоскости (ЛИНИИ СЕТКИ - статично)
            DrawXYPlane(canvas, centerX, centerY, dynamicGridStep, fixedCellSize); // <- fixedCellSize
            
            // 3. Рисуем поверхность функции (масштабируется)
            DrawFunctionSurface(canvas, centerX, centerY, fixedCellSize);

            // 4. Рисуем оси координат (статично)
            DrawAxis(canvas, -CubeSize, 0, 0, CubeSize, 0, 0, Brushes.Red, "X", centerX, centerY, fixedCellSize);
            DrawAxis(canvas, 0, -CubeSize, 0, 0, CubeSize, 0, Brushes.Green, "Y", centerX, centerY, fixedCellSize);
            DrawAxis(canvas, 0, 0, -CubeSize, 0, 0, CubeSize, Brushes.Blue, "Z", centerX, centerY, fixedCellSize);
        }
        private void DrawCubeFrame(Canvas canvas, double centerX, double centerY, double cellSize)
        {
            Color frameColor = Colors.Black;
            double thickness = 1.5;

            // Нижняя грань (z = -GridSize)
            DrawFrameLine(canvas, -CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, -CubeSize, -CubeSize, CubeSize, CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -CubeSize, CubeSize, -CubeSize, -CubeSize, -CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Верхняя грань (z = CubeSize)
            DrawFrameLine(canvas, -CubeSize, -CubeSize, CubeSize, CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, -CubeSize, CubeSize, CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, CubeSize, CubeSize, -CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -CubeSize, CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Вертикальные рёбра
            DrawFrameLine(canvas, -CubeSize, -CubeSize, -CubeSize, -CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, CubeSize, CubeSize, -CubeSize, CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
        }

        private void DrawFrameLine(Canvas canvas, 
                                 double x1, double y1, double z1,
                                 double x2, double y2, double z2,
                                 Color color, double thickness,
                                 double centerX, double centerY, double cellSize)
        {
            var p1 = ProjectTo2D(x1, y1, z1, cellSize);
            var p2 = ProjectTo2D(x2, y2, z2, cellSize);
            
            var line = new Line
            {
                StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            };
            canvas.Children.Add(line);
        }

        private void DrawXYPlane(Canvas canvas, double centerX, double centerY, double gridStep, double cellSize)
        {
            Color gridColor = Color.FromArgb(80, 150, 150, 150);
            gridStep = (GridSize * 2)/4;
            // Линии по X (z=0)
            for (double x = -GridSize; x <= GridSize; x += gridStep)
            {
                var x_norm = x/GridSize * CubeSize;
                 // Линии рисуем в фиксированном масштабе
                var p1 = ProjectTo2D(x_norm, -CubeSize, 0, cellSize);
                var p2 = ProjectTo2D(x_norm, CubeSize, 0, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);

                // Подписи осей масштабируются (используем gridCellSize)
                var labelPos = ProjectTo2D(x_norm, -0.1, 0, cellSize); // Масштабируем!
                DrawLabel(canvas, (x).ToString(), Brushes.Black, 
                labelPos, centerX, centerY, horizontal: true);
            }

            // Линии по Y (z=0)
            for (double y = -GridSize; y <= GridSize; y += gridStep)
            {
                var y_norm = y/GridSize * CubeSize;
                var p1 = ProjectTo2D(-CubeSize, y_norm, 0, cellSize);
                var p2 = ProjectTo2D(CubeSize, y_norm, 0, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
                // Подписи оси Y
                var labelPos = ProjectTo2D(0, y_norm, 0, cellSize);
                DrawLabel(canvas, y.ToString(), Brushes.Black, labelPos, 
                centerX, centerY, horizontal: false);
            }

        }
        private void DrawLabel(Canvas canvas, string text, IBrush color, Point position, 
                             double centerX, double centerY, bool horizontal = true)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = 10,
                FontWeight = FontWeight.Bold
            };

            if (horizontal)
            {
                Canvas.SetLeft(textBlock, position.X + centerX - 5);
                Canvas.SetTop(textBlock, position.Y + centerY + 2);
            }
            else
            {
                Canvas.SetLeft(textBlock, position.X + centerX + 2);
                Canvas.SetTop(textBlock, position.Y + centerY - 5);
            }

            canvas.Children.Add(textBlock);
        }

        private void DrawFunctionSurface(Canvas canvas, double centerX, double centerY, double cellSize)
        {
            double step = GridSize / 10;
            Color surfaceColor = Color.FromArgb(180, 0, 120, 215);

            // Масштабируем размер функции в соответствии с GridSize
            double scaleFactor = CubeSize / GridSize;

            for (double x = -GridSize; x < GridSize; x += step)
            {
                for (double y = -GridSize; y < GridSize; y += step)
                {
                    // Координаты в масштабе функции
                    double x1 = x * scaleFactor;
                    double y1 = y * scaleFactor;
                    double x2 = (x + step) * scaleFactor;
                    double y2 = (y + step) * scaleFactor;
                    
                    // Вычисляем z для каждой точки
                    double z1 = Math.Clamp(Function(x, y), -GridSize, GridSize) * scaleFactor;
                    double z2 = Math.Clamp(Function(x + step, y), -GridSize, GridSize) * scaleFactor;
                    double z3 = Math.Clamp(Function(x + step, y + step), -GridSize, GridSize) * scaleFactor;
                    double z4 = Math.Clamp(Function(x, y + step), -GridSize, GridSize) * scaleFactor;

                    // Проецируем точки в 2D
                    var p1 = ProjectTo2D(x1, y1, z1, cellSize);
                    var p2 = ProjectTo2D(x2, y1, z2, cellSize);
                    var p3 = ProjectTo2D(x2, y2, z3, cellSize);
                    var p4 = ProjectTo2D(x1, y2, z4, cellSize);

                    // Рисуем два треугольника
                    var triangle1 = new Polygon
                    {
                        Points = new Points {
                            new Point(p1.X + centerX, p1.Y + centerY),
                            new Point(p2.X + centerX, p2.Y + centerY),
                            new Point(p3.X + centerX, p3.Y + centerY)
                        },
                        Fill = new SolidColorBrush(surfaceColor),
                        Stroke = Brushes.Transparent
                    };

                    var triangle2 = new Polygon
                    {
                        Points = new Points {
                            new Point(p1.X + centerX, p1.Y + centerY),
                            new Point(p3.X + centerX, p3.Y + centerY),
                            new Point(p4.X + centerX, p4.Y + centerY)
                        },
                        Fill = new SolidColorBrush(surfaceColor),
                        Stroke = Brushes.Transparent
                    };

                    canvas.Children.Add(triangle1);
                    canvas.Children.Add(triangle2);
                }
            }
        }

        private struct Point3D
        {
            public double X { get; }
            public double Y { get; }
            public double Z { get; }

            public Point3D(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        private double Function(double x, double y)
        {
            return x;
        }

        private TextBlock CreateLabel(string text, IBrush color, double fontSize)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = color,
                FontSize = fontSize,
                FontWeight = FontWeight.Normal
            };
        }
private void DrawAxis(Canvas canvas, 
                    double x1, double y1, double z1,
                    double x2, double y2, double z2,
                    IBrush color, string label,
                    double centerX, double centerY, double cellSize)
        {
            var start = ProjectTo2D(x1, y1, z1, cellSize);
            var end = ProjectTo2D(x2, y2, z2, cellSize);
            
            var line = new Line
            {
                StartPoint = new Point(start.X + centerX, start.Y + centerY),
                EndPoint = new Point(end.X + centerX, end.Y + centerY),
                Stroke = color,
                StrokeThickness = 2
            };
            canvas.Children.Add(line);

            // Добавляем подпись оси (статичную)
            var textBlock = CreateLabel(label, color, 16);
            textBlock.FontWeight = FontWeight.Bold;
            
            canvas.Children.Add(textBlock);
            Canvas.SetLeft(textBlock, end.X + centerX + (label == "X" ? 10 : 0));
            Canvas.SetTop(textBlock, end.Y + centerY - (label == "Z" ? 20 : 0));
        }

        private Point ProjectTo2D(double x, double y, double z, double cellSize)
        {
            // Изометрическая проекция с Z как вертикальной осью (направленной вверх)
            double x2d = x * Math.Cos(_angleY) + y * Math.Sin(_angleY);
            double y2d = x * Math.Sin(_angleX) * Math.Sin(_angleY) - 
                        y * Math.Sin(_angleX) * Math.Cos(_angleY) - 
                        z * Math.Cos(_angleX);
            
            return new Point(x2d * cellSize, y2d * cellSize);
        }
    }
}