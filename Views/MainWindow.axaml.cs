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
        private const int GridSize = 10;
        private double _cellSize = 40;
        private Point _lastMousePosition;
        private double _angleX = 35 * Math.PI / 180;
        private double _angleY = -45 * Math.PI / 180;
        private bool _isDragging;
        private double _zoom = 1.0;
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
            _zoom *= zoomFactor;
            _zoom = Math.Clamp(_zoom, 0.2, 5.0);
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
            double gridCellSize = fixedCellSize / _zoom;

            // Адаптивный шаг сетки (увеличивается при уменьшении масштаба)
            int dynamicGridStep = (int)Math.Max(1, Math.Round(2 * _zoom));
            
            // 1. Рисуем кубический каркас (статично)
            DrawCubeFrame(canvas, centerX, centerY, fixedCellSize);
            
            // 2. Рисуем координатные плоскости (ЛИНИИ СЕТКИ - статично)
            DrawXYPlane(canvas, centerX, centerY, dynamicGridStep, fixedCellSize); // <- fixedCellSize
            //DrawXZPlane(canvas, centerX, centerY, dynamicGridStep, fixedCellSize); // <- fixedCellSize
            //DrawYZPlane(canvas, centerX, centerY, dynamicGridStep, fixedCellSize); // <- fixedCellSize
            
            // 3. Рисуем поверхность функции (масштабируется)
            DrawFunctionSurface(canvas, centerX, centerY, gridCellSize);

            // 4. Рисуем оси координат (статично)
            DrawAxis(canvas, -GridSize, 0, 0, GridSize, 0, 0, Brushes.Red, "X", centerX, centerY, fixedCellSize);
            DrawAxis(canvas, 0, -GridSize, 0, 0, GridSize, 0, Brushes.Green, "Y", centerX, centerY, fixedCellSize);
            DrawAxis(canvas, 0, 0, -GridSize*2, 0, 0, GridSize*2, Brushes.Blue, "Z", centerX, centerY, fixedCellSize);
        }
        private void DrawCubeFrame(Canvas canvas, double centerX, double centerY, double cellSize)
        {
            Color frameColor = Colors.Black;
            double thickness = 1.5;

            // Нижняя грань (z = -GridSize)
            DrawFrameLine(canvas, -GridSize, -GridSize, -GridSize, GridSize, -GridSize, -GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, -GridSize, -GridSize, GridSize, GridSize, -GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, GridSize, -GridSize, -GridSize, GridSize, -GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -GridSize, GridSize, -GridSize, -GridSize, -GridSize, -GridSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Верхняя грань (z = GridSize)
            DrawFrameLine(canvas, -GridSize, -GridSize, GridSize, GridSize, -GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, -GridSize, GridSize, GridSize, GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, GridSize, GridSize, -GridSize, GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -GridSize, GridSize, GridSize, -GridSize, -GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Вертикальные рёбра
            DrawFrameLine(canvas, -GridSize, -GridSize, -GridSize, -GridSize, -GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, -GridSize, -GridSize, GridSize, -GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, GridSize, GridSize, -GridSize, GridSize, GridSize, GridSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(canvas, -GridSize, GridSize, -GridSize, -GridSize, GridSize, GridSize
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

        private void DrawXYPlane(Canvas canvas, double centerX, double centerY, int gridStep, double cellSize)
        {
            Color gridColor = Color.FromArgb(80, 150, 150, 150);

            // Линии по X (z=0)
            for (int x = -GridSize; x <= GridSize; x += gridStep)
            {
                 // Линии рисуем в фиксированном масштабе
                var p1 = ProjectTo2D(x, -GridSize, 0, cellSize);
                var p2 = ProjectTo2D(x, GridSize, 0, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);

                // Подписи осей масштабируются (используем gridCellSize)
                if (x % (gridStep * 2) == 0 && x != 0)
                {
                    var labelPos = ProjectTo2D(x, -0.1, 0, cellSize / _zoom); // Масштабируем!
                    DrawLabel(canvas, (x * _zoom).ToString("0"), Brushes.Black, 
                            labelPos, centerX, centerY, horizontal: true);
                }
            }

            // Линии по Y (z=0)
            for (int y = -GridSize; y <= GridSize; y += gridStep)
            {
                var p1 = ProjectTo2D(-GridSize, y, 0, cellSize);
                var p2 = ProjectTo2D(GridSize, y, 0, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);

                // Подписи оси Y
                if (y % (gridStep * 2) == 0 && y != 0)
                {
                    var labelPos = ProjectTo2D(0, y, 0, cellSize / _zoom);
                    DrawLabel(canvas, y.ToString(), Brushes.Black, labelPos, centerX, centerY, horizontal: false);
                }
            }
        }

        private void DrawXZPlane(Canvas canvas, double centerX, double centerY, int gridStep, double cellSize)
        {
            Color gridColor = Color.FromArgb(80, 200, 150, 150);

            // Линии по X (y=0)
            for (int x = -GridSize; x <= GridSize; x += gridStep)
            {
                var p1 = ProjectTo2D(x, 0, -GridSize, cellSize);
                var p2 = ProjectTo2D(x, 0, GridSize, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }

            // Линии по Z (y=0)
            for (int z = -GridSize; z <= GridSize; z += gridStep)
            {
                var p1 = ProjectTo2D(-GridSize, 0, z, cellSize);
                var p2 = ProjectTo2D(GridSize, 0, z, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);

                // Подписи оси Z
                if (z % (gridStep * 2) == 0 && z != 0)
                {
                    var labelPos = ProjectTo2D(0, 0, z, cellSize);
                    DrawLabel(canvas, z.ToString(), Brushes.Black, labelPos, centerX, centerY, horizontal: false);
                }
            }
        }

        private void DrawYZPlane(Canvas canvas, double centerX, double centerY, int gridStep, double cellSize)
        {
            Color gridColor = Color.FromArgb(80, 150, 200, 150);

            // Линии по Y (x=0)
            for (int y = -GridSize; y <= GridSize; y += gridStep)
            {
                var p1 = ProjectTo2D(0, y, -GridSize, cellSize);
                var p2 = ProjectTo2D(0, y, GridSize, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }

            // Линии по Z (x=0)
            for (int z = -GridSize; z <= GridSize; z += gridStep)
            {
                var p1 = ProjectTo2D(0, -GridSize, z, cellSize);
                var p2 = ProjectTo2D(0, GridSize, z, cellSize);
                
                var line = new Line
                {
                    StartPoint = new Point(p1.X + centerX, p1.Y + centerY),
                    EndPoint = new Point(p2.X + centerX, p2.Y + centerY),
                    Stroke = new SolidColorBrush(gridColor),
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
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
                Canvas.SetTop(textBlock, position.Y + centerY - 6);
            }

            canvas.Children.Add(textBlock);
        }

        private void DrawFunctionSurface(Canvas canvas, double centerX, double centerY, double cellSize)
        {
            double step = 0.5;
            Color surfaceColor = Color.FromArgb(180, 0, 120, 215);

            for (double x = -GridSize; x < GridSize; x += step)
            {
                for (double y = -GridSize; y < GridSize; y += step)
                {
                    // Получаем четыре точки квадрата
                    double x1 = x;
                    double y1 = y;
                    double x2 = x + step;
                    double y2 = y + step;

                    // Вычисляем z для каждой точки
                    double z1 = Function(x1, y1);
                    double z2 = Function(x2, y1);
                    double z3 = Function(x2, y2);
                    double z4 = Function(x1, y2);

                    // Собираем все точки квадрата
                    List<Point3D> square = new List<Point3D>
                    {
                        new Point3D(x1, y1, z1),
                        new Point3D(x2, y1, z2),
                        new Point3D(x2, y2, z3),
                        new Point3D(x1, y2, z4)
                    };

                    // Проверяем, находится ли хотя бы одна точка внутри куба
                    bool hasVisiblePoints = square.Any(p => 
                        p.Z >= -GridSize && p.Z <= GridSize);

                    if (!hasVisiblePoints)
                        continue;

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

        private double Function(double _x, double _y)
        {
            double x = _x * _zoom;
            double y = _y * _zoom;
            
            // Функция z = x + y (z - вертикальная ось)
            return x*x + y*y;
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