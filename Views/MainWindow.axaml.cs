using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System.Linq;
using GoodbyeDiplom.ViewModels;
using GoodbyeDiplom.Views;
using ReactiveUI;
using Avalonia.Threading;
namespace GoodbyeDiplom.Views
{
    public partial class MainWindow : Window
    {
        private double GridSize = 5;
        private const int CubeSize = 6;
        private double _cellSize = 40;
        private Point _lastMousePosition;
        private double _angleX = 35 * Math.PI / 180;
        private double _angleY = -45 * Math.PI / 180;
        private bool _isDragging;
        Canvas canvas;
        //private double _zoom = 1.0;
        private List<Shape> _surfaceTriangles = new List<Shape>();
        public MainWindow()
        {
            InitializeComponent();
            Title = "3D Function Surface (Z Vertical)";
            Width = 800;
            Height = 600;
            var vm = new MainWindowViewModel();
            DataContext = vm;
            // В конструкторе MainWindow
            canvas = this.FindControl<Canvas>("MainCanvas");
            this.PointerPressed += OnPointerPressed;
            this.PointerReleased += OnPointerReleased;
            this.PointerMoved += OnPointerMoved;
            this.PointerWheelChanged += OnPointerWheelChanged;
            this.SizeChanged += OnSizeChanged;
            PointerMoved += (s, e) => UpdateMouseCoordinates(e);
            var button = this.FindControl<Button>("CreateFunction");
            button.Click += (s, e) => 
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    vm.UpdateFunction();
                    UpdateGrid();
                }
            };
        }
        private void UpdateGrid()
        {
            canvas.Children.Clear();
            _surfaceTriangles.Clear();
            double centerX = (Bounds.Width - 200) / 2;
            double centerY = Bounds.Height / 2;

            // Фиксированный размер для куба, осей и СЕТКИ
            double fixedCellSize = Math.Min(Bounds.Width - 200, Bounds.Height) / 20;
            
            // Масштабируемый размер только для функции и числовых подписей
            //double gridCellSize = fixedCellSize / _zoom;

            // Адаптивный шаг сетки (увеличивается при уменьшении масштаба)
            double DynamicStep = DynStepCalc(GridSize);
            
            // 1. Рисуем кубический каркас (статично)
            DrawCubeFrame(centerX, centerY, fixedCellSize);
            
            // 2. Рисуем координатные плоскости (ЛИНИИ СЕТКИ - статично)
            DrawXYPlane(centerX, centerY, DynamicStep, fixedCellSize); // <- fixedCellSize
            
            // 3. Рисуем поверхность функции (масштабируется)
            DrawFunctionSurface(centerX, centerY, fixedCellSize);

            // 4. Рисуем оси координат (статично)
            DrawAxis(-CubeSize, 0, 0, CubeSize, 0, 0, Brushes.Red, "X", centerX, centerY, fixedCellSize);
            DrawAxis(0, -CubeSize, 0, 0, CubeSize, 0, Brushes.Green, "Y", centerX, centerY, fixedCellSize);
            DrawAxis(0, 0, -CubeSize, 0, 0, CubeSize, Brushes.Blue, "Z", centerX, centerY, fixedCellSize);
        }
        private void UpdateMouseCoordinates(PointerEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;

            var position = e.GetPosition(canvas);
            var centerX = canvas.Bounds.Width / 2;
            var centerY = canvas.Bounds.Height / 2;
            
            var mouseX = position.X - centerX;
            var mouseY = position.Y - centerY;

            // Определяем ближайшую ось
            var nearestAxis = GetNearestAxis(mouseX, mouseY, centerX, centerY);
            
            string xCoord = "—";
            string yCoord = "—";
            string zCoord = "—";

            if (nearestAxis != null)
            {
                double value = GetAxisValue(mouseX, mouseY, nearestAxis, centerX, centerY);
                string formattedValue = value.ToString("0.##");

                switch (nearestAxis)
                {
                    case "X":
                        xCoord = formattedValue;
                        // Если мы на оси X, попробуем найти Z (вертикальная координата)
                        zCoord = GetZValueOnXAxis(mouseX, mouseY, centerX, centerY, value);
                        break;
                    case "Y":
                        yCoord = formattedValue;
                        // Если мы на оси Y, попробуем найти Z (вертикальная координата)
                        zCoord = GetZValueOnYAxis(mouseX, mouseY, centerX, centerY, value);
                        break;
                    case "Z":
                        zCoord = formattedValue;
                        break;
                }
            }

            vm.GridCoordinates = $"X: {xCoord}, Y: {yCoord}, Z: {zCoord}";
        }

        private string GetZValueOnXAxis(double mouseX, double mouseY, double centerX, double centerY, double xValue)
        {
            // Проецируем точку на оси и смотрим высоту
            double step = GridSize / 10;
            double closestDist = double.MaxValue;
            double closestZ = 0;
            
            for (double z = -GridSize; z <= GridSize; z += step)
            {
                var point = ProjectTo2D(
                    xValue / GridSize * CubeSize, 
                    0, 
                    z / GridSize * CubeSize, 
                    _cellSize);
                
                double dist = Math.Sqrt(Math.Pow(point.X - mouseX, 2) + Math.Pow(point.Y - mouseY, 2));
                
                if (dist < closestDist && dist < 20) // 20 пикселей - радиус поиска
                {
                    closestDist = dist;
                    closestZ = z;
                }
            }
            
            return closestDist < 20 ? closestZ.ToString("0.##") : "—";
        }

        private string GetZValueOnYAxis(double mouseX, double mouseY, double centerX, double centerY, double yValue)
        {
            // Аналогично для оси Y
            double step = GridSize / 10;
            double closestDist = double.MaxValue;
            double closestZ = 0;
            
            for (double z = -GridSize; z <= GridSize; z += step)
            {
                var point = ProjectTo2D(
                    0, 
                    yValue / GridSize * CubeSize, 
                    z / GridSize * CubeSize, 
                    _cellSize);
                
                double dist = Math.Sqrt(Math.Pow(point.X - mouseX, 2) + Math.Pow(point.Y - mouseY, 2));
                
                if (dist < closestDist && dist < 20)
                {
                    closestDist = dist;
                    closestZ = z;
                }
            }
            
            return closestDist < 20 ? closestZ.ToString("0.##") : "—";
        }
                private double GetAxisValue(double mouseX, double mouseY, string axis, double centerX, double centerY)
        {
            // Преобразуем координаты мыши в значение на оси
            double value = 0;
            
            switch (axis)
            {
                case "X":
                    var xStart = ProjectTo2D(-CubeSize, 0, 0, _cellSize);
                    var xEnd = ProjectTo2D(CubeSize, 0, 0, _cellSize);
                    double t = GetInterpolationFactor(mouseX, mouseY, xStart.X, xStart.Y, xEnd.X, xEnd.Y);
                    value = Lerp(-GridSize, GridSize, t);
                    break;
                    
                case "Y":
                    var yStart = ProjectTo2D(0, -CubeSize, 0, _cellSize);
                    var yEnd = ProjectTo2D(0, CubeSize, 0, _cellSize);
                    t = GetInterpolationFactor(mouseX, mouseY, yStart.X, yStart.Y, yEnd.X, yEnd.Y);
                    value = Lerp(-GridSize, GridSize, t);
                    break;
                    
                case "Z":
                    var zStart = ProjectTo2D(0, 0, -CubeSize, _cellSize);
                    var zEnd = ProjectTo2D(0, 0, CubeSize, _cellSize);
                    t = GetInterpolationFactor(mouseX, mouseY, zStart.X, zStart.Y, zEnd.X, zEnd.Y);
                    value = Lerp(-GridSize, GridSize, t);
                    break;
            }
            
            return value;
        }
        private string GetNearestAxis(double mouseX, double mouseY, double centerX, double centerY)
        {
            const double axisThreshold = 15; // Расстояние в пикселях для захвата оси
            
            // Создаем список осей с их параметрами
            var axes = new List<(string Name, Point Start, Point End, IBrush Color)>
            {
                ("X", 
                ProjectTo2D(-CubeSize, 0, 0, _cellSize), 
                ProjectTo2D(CubeSize, 0, 0, _cellSize), 
                Brushes.Red),
                ("Y", 
                ProjectTo2D(0, -CubeSize, 0, _cellSize), 
                ProjectTo2D(0, CubeSize, 0, _cellSize), 
                Brushes.Green),
                ("Z", 
                ProjectTo2D(0, 0, -CubeSize, _cellSize), 
                ProjectTo2D(0, 0, CubeSize, _cellSize), 
                Brushes.Blue)
            };

            // Находим ближайшую ось
            var nearestAxis = axes
                .Select(axis => new {
                    Axis = axis.Name,
                    Distance = DistanceToLine(mouseX, mouseY, axis.Start.X, axis.Start.Y, axis.End.X, axis.End.Y)
                })
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            return nearestAxis?.Distance <= axisThreshold ? nearestAxis.Axis : null;
        }
        private double DistanceToLine(double x, double y, double x1, double y1, double x2, double y2)
        {
            // Вычисляем расстояние от точки (x,y) до линии (x1,y1)-(x2,y2)
            double A = x - x1;
            double B = y - y1;
            double C = x2 - x1;
            double D = y2 - y1;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * C;
                yy = y1 + param * D;
            }

            return Math.Sqrt((x - xx) * (x - xx) + (y - yy) * (y - yy));
        }
        private double GetInterpolationFactor(double x, double y, double x1, double y1, double x2, double y2)
        {
            // Вычисляем параметр t (0-1) для точки на линии
            double dx = x2 - x1;
            double dy = y2 - y1;
            
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                return (x - x1) / dx;
            }
            else
            {
                return (y - y1) / dy;
            }
        }

        private double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }
        // Команды для кнопок
        public void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            GridSize *= 0.9;
            UpdateGrid();
        }

        public void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            GridSize *= 1.1;
            UpdateGrid();
        }

        public void RotateRight_Click(object sender, RoutedEventArgs e)
        {
            _angleY -= 0.1;
            UpdateGrid();
        }

        public void RotateLeft_Click(object sender, RoutedEventArgs e)
        {
            _angleY += 0.1;
            UpdateGrid();
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
            // if (DataContext is MainWindowViewModel vm)
            // {
            //     vm.UpdateMouseCoordinates(e, this);
            // }
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
        private double DynStepCalc(double gridSize)
        {
            // Определяем текущий "уровень масштаба" (0, 1, 2, 0, 1, 2...)
            int scaleLevel = (int)(Math.Log(gridSize, 2)) % 2;
            
            // Возвращаем шаг: 2^1=2, 2^2=4, 2^3=8, затем снова 2...
            return Math.Pow(2, scaleLevel + 1);
        }
        private void DrawCubeFrame(double centerX, double centerY, double cellSize)
        {
            Color frameColor = Colors.Black;
            double thickness = 1.5;

            // Нижняя грань (z = -GridSize)
            DrawFrameLine(-CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, -CubeSize, -CubeSize, CubeSize, CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(-CubeSize, CubeSize, -CubeSize, -CubeSize, -CubeSize, -CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Верхняя грань (z = CubeSize)
            DrawFrameLine(-CubeSize, -CubeSize, CubeSize, CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, -CubeSize, CubeSize, CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, CubeSize, CubeSize, -CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(-CubeSize, CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);

            // Вертикальные рёбра
            DrawFrameLine(-CubeSize, -CubeSize, -CubeSize, -CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, -CubeSize, -CubeSize, CubeSize, -CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(CubeSize, CubeSize, -CubeSize, CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
            DrawFrameLine(-CubeSize, CubeSize, -CubeSize, -CubeSize, CubeSize, CubeSize
            , frameColor, thickness, centerX, centerY, cellSize);
        }

        private void DrawFrameLine( 
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

        private void DrawXYPlane(double centerX, double centerY, double gridStep, double cellSize)
        {
            Color gridColor = Color.FromArgb(80, 150, 150, 150);
            gridStep = GridSize/gridStep;
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
                DrawLabel((x).ToString("0.##"), Brushes.Black, 
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
                DrawLabel(y.ToString("0.##"), Brushes.Black, labelPos, 
                centerX, centerY, horizontal: false);
            }

        }
        private void DrawLabel(string text, IBrush color, Point position, 
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

        private void DrawFunctionSurface(double centerX, double centerY, double cellSize)
        {
            if (!(DataContext is MainWindowViewModel vm)) return;

            double step = GridSize / 10;
            Color surfaceColor = Color.FromArgb(180, 0, 120, 215);
            double discontinuityThreshold = GridSize * 2;
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

                    double z1 = SafeCalculate(vm, x, y, discontinuityThreshold);
                    double z2 = SafeCalculate(vm, x + step, y, discontinuityThreshold);
                    double z3 = SafeCalculate(vm, x + step, y + step, discontinuityThreshold);
                    double z4 = SafeCalculate(vm, x, y + step, discontinuityThreshold);

                    // Пропускаем полигон, если обнаружен разрыв
                    if (double.IsInfinity(z1)) continue;
                    if (double.IsInfinity(z2)) continue;
                    if (double.IsInfinity(z3)) continue;
                    if (double.IsInfinity(z4)) continue;

                    // Вычисляем z для каждой точки
                    z1 = Math.Clamp(z1, -GridSize, GridSize) * scaleFactor;
                    z2 = Math.Clamp(z2, -GridSize, GridSize) * scaleFactor;
                    z3 = Math.Clamp(z3, -GridSize, GridSize) * scaleFactor;
                    z4 = Math.Clamp(z4, -GridSize, GridSize) * scaleFactor;

                    // Проецируем точки в 2D
                    var p1 = ProjectTo2D(x1, y1, z1, cellSize);
                    var p2 = ProjectTo2D(x2, y1, z2, cellSize);
                    var p3 = ProjectTo2D(x2, y2, z3, cellSize);
                    var p4 = ProjectTo2D(x1, y2, z4, cellSize);
                    //Обработка разрывов
                    if (!ShouldSkipPolygon(p1, p2, p3, p4, cellSize * 10)) // Увеличили порог пропуска
                    {
                        var triangle1 = new Polygon
                        {
                            Points = new Points {
                                new Point(p1.X + centerX, p1.Y + centerY),
                                new Point(p2.X + centerX, p2.Y + centerY),
                                new Point(p3.X + centerX, p3.Y + centerY)
                            },
                            Fill = new SolidColorBrush(surfaceColor),
                            Stroke = Brushes.Transparent,
                            Opacity = double.IsInfinity(z1) || double.IsInfinity(z2) || double.IsInfinity(z3) ? 0.5 : 1
                        };

                        var triangle2 = new Polygon
                        {
                            Points = new Points {
                                new Point(p1.X + centerX, p1.Y + centerY),
                                new Point(p3.X + centerX, p3.Y + centerY),
                                new Point(p4.X + centerX, p4.Y + centerY)
                            },
                            Fill = new SolidColorBrush(surfaceColor),
                            Stroke = Brushes.Transparent,
                            Opacity = double.IsInfinity(z1) || double.IsInfinity(z3) || double.IsInfinity(z4) ? 0.5 : 1
                        };

                        canvas.Children.Add(triangle1);
                        canvas.Children.Add(triangle2);
                    }
                }
            }
        }
        private double SafeCalculate(MainWindowViewModel vm, double x, double y, double threshold)
        {
            try
            {
                double result = vm.CalculateFunction(x, y);
                
                // Проверяем на слишком большие значения (разрывы)
                if (double.IsInfinity(result) || Math.Abs(result) > threshold)
                    return double.PositiveInfinity;
                    
                return result;
            }
            catch
            {
                return double.PositiveInfinity;
            }
        }
        private bool ShouldSkipPolygon(Point p1, Point p2, Point p3, Point p4, double maxDistance)
        {
            // Увеличиваем максимальное допустимое расстояние между точками
            if (DistanceBetween(p1, p2) > maxDistance) return true;
            if (DistanceBetween(p2, p3) > maxDistance) return true;
            if (DistanceBetween(p3, p4) > maxDistance) return true;
            if (DistanceBetween(p4, p1) > maxDistance) return true;
            return false;
        }

        private double DistanceBetween(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
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
            return Math.Exp(x) + Math.Exp(y);
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
        private void DrawAxis(
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