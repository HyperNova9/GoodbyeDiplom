#pragma warning disable CS8618, CS8622, CS8601, CS8602, CS8603
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
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
using System.Threading.Tasks;
using System.Reactive.Linq;
using DynamicData.Binding;
using System.Threading;
using Avalonia.Media.Imaging;
using System.Text.Json;
using System.IO;
using System.Collections.ObjectModel;
using Avalonia.Input.GestureRecognizers;
namespace GoodbyeDiplom.Views
{
    public partial class MainWindow : Window
    {
        //Инициализация данных
        private double GridSize = 5;
        private bool _scaleCube = false;
        private int CubeSize = 7;
        private double _cellSize = 40;
        private Point _lastMousePosition;
        private ICollection<Polygons> polygons;
        private double _angleX = 35 * Math.PI / 180;
        private double _angleY = -45 * Math.PI / 180;
        private bool _isDragging;
        private bool _isMoved = false;
        private bool _isWheeling = false;
        private DispatcherTimer WheelTimer;
        Canvas canvas;
        public MainWindow()
        {
            InitializeComponent();
            polygons = new Collection<Polygons>();
            canvas = MainCanvas;
            Width = 800;
            Height = 600;
            var vm = new MainWindowViewModel();
            DataContext = vm;

            PointerPressed += OnPointerPressed;
            PointerReleased += OnPointerReleased;
            PointerMoved += OnPointerMoved;
            PointerWheelChanged += OnPointerWheelChanged;
            SizeChanged += OnSizeChanged;
            
            PointerMoved += (s, e) => UpdateMouseCoordinates(e);
            OnChangedValues();
        }
        private void ColorsSceneWindow_Click(object sender, RoutedEventArgs e)
        {
            var colorsScene = new ColorsScene(this)
            {
                DataContext = this.DataContext // Передаём тот же DataContext
            };
            colorsScene.Show();
        }
        private void OnChangedValues()
        {
            if (DataContext is not MainWindowViewModel vm) return;
            vm.UpdateData += UpdateGridEventHandler;
        }
        //Перерисовка и отрисовка сетки
        public void UpdateGrid()
        {
            if (DataContext is not MainWindowViewModel vm) return;
            canvas.Background = new SolidColorBrush(vm.ColorsScene.ColorBG);
            canvas.Children.Clear();
            polygons.Clear();
            double centerX = (Bounds.Width - 300) / 2;
            double centerY = Bounds.Height / 2;

            // Фиксированный размер для куба, осей и СЕТКИ
            double fixedCellSize = Math.Min(Bounds.Width - 300, Bounds.Height) / 20;

            // Адаптивный шаг сетки (увеличивается при уменьшении масштаба)
            double DynamicStep = DynStepCalc(GridSize);
            

            
            
            if (vm.ShowAxes)
            {
                DrawAxis(-CubeSize, 0, 0, CubeSize, 0, 0, vm.ColorsScene.ColorX, "X",
                    centerX, centerY, fixedCellSize);
                DrawAxis(0, -CubeSize, 0, 0, CubeSize, 0, vm.ColorsScene.ColorY, "Y",
                centerX, centerY, fixedCellSize);
                DrawAxis(0, 0, -CubeSize, 0, 0, CubeSize, vm.ColorsScene.ColorZ, "Z",
                centerX, centerY, fixedCellSize);
            }
            // 3. Рисуем поверхность(-и) функции(-й) (масштабируется)
            if (vm.ShowGraphic)
            {
                var temp = vm.FunctionExpression;
                foreach (var f in vm.Functions)
                {
                    if (f.IsVisible && f != vm.SelectedFunction)
                    {
                        vm.FunctionExpression = f.Expression;
                        vm.UpdateFunction();
                        DrawFunctionSurface(centerX, centerY, fixedCellSize, f);
                    }
                    
                }
                if (vm.SelectedFunction != null && vm.SelectedFunction.IsVisible) 
                {
                    vm.FunctionExpression = vm.SelectedFunction.Expression;
                    vm.UpdateFunction();
                    DrawFunctionSurface(centerX, centerY, fixedCellSize, vm.SelectedFunction);
                }  
                vm.FunctionExpression = temp;
                vm.UpdateFunction();
       
            }
            PolygonsSort();
            Console.WriteLine(polygons.Count); 
            ShowPolygons();
                        // 1. Рисуем координатные плоскости (ЛИНИИ СЕТКИ - статично)
            if (vm.ShowGrid)
                DrawXYPlane(centerX, centerY, DynamicStep, fixedCellSize); // <- fixedCellSize
            if (vm.ShowLabels)
            {
                DrawAxisLabel(-CubeSize, 0, 0, CubeSize, 0, 0, vm.ColorsScene.ColorX, "X",
                    centerX, centerY, fixedCellSize);
                DrawAxisLabel(0, -CubeSize, 0, 0, CubeSize, 0, vm.ColorsScene.ColorY, "Y",
                centerX, centerY, fixedCellSize);
                DrawAxisLabel(0, 0, -CubeSize, 0, 0, CubeSize, vm.ColorsScene.ColorZ, "Z",
                centerX, centerY, fixedCellSize);
            }
            // 4. Рисуем кубический каркас (статично)
            if (vm.ShowCube)
                DrawCubeFrame(centerX, centerY, fixedCellSize);
        }
        private void ShowPolygons()
        {
            if (DataContext is not MainWindowViewModel vm) return;
            foreach (var poly in polygons)
            {
                canvas.Children.Add(poly.triangle);
            }
        }

        private bool PolygonsIntersect(Polygons poly1, Polygon existing)
        {
            // Получаем точки полигонов
            var middleP = poly1.middlePoint;
            var CameraPos = CalculateCameraPosition();
            var Vec = new Point3D(CameraPos.X - middleP.X, 
            CameraPos.Y - middleP.Y, CameraPos.Z - middleP.Z);
            
            var p = existing.Points.ToArray();
            // var V1 = new Point3D(p[1].X - p[0].X, p[1].Y - p[0].Y, p[1].Z - p[0].Z);
            return false;
        }
        
        private void PolygonsSort()
        {
            polygons = polygons.OrderByDescending(p => p.distance).ToList();
        }

        private void OnFunctionExpressionChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                vm.SelectedFunction.Expression = ((TextBox)sender).Text;
                //vm.FunctionExpression = vm.SelectedFunction.Expression;
                vm.UpdateFunction();
                UpdateGrid();
            }
        }
        private void OnFunctionStepSizeChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                vm.SelectedFunction.StepSize = ((Slider)sender).Value;
                vm.StepSize = vm.SelectedFunction.StepSize;
                vm.UpdateFunction();
                UpdateGrid();
            }
        }
        private void OnFunctionStepColorChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                vm.SelectedFunction.StepColor = ((Slider)sender).Value;
                vm.StepColor = vm.SelectedFunction.StepColor;
                vm.UpdateFunction();
                UpdateGrid();
            }
        }
        private void OnFunctionColorStartChanged(object sender, ColorChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                vm.SelectedFunction.ColorStart = e.NewColor;
                vm.StartColor = e.NewColor;
                UpdateGrid();
            }
        }
        private void OnFunctionColorEndChanged(object sender, ColorChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                vm.SelectedFunction.ColorEnd = e.NewColor;
                vm.EndColor = e.NewColor;
                UpdateGrid();
            }
        }
        private void OnColorChanged(object sender, ColorChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedFunction != null)
            {
                UpdateGrid();
            }
        }
        private void UpdateGridFromVM(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            UpdateGrid();
        }
        private void UpdateGridEventHandler(object? sender, double isEvent)
        {
            UpdateGrid();
        }

        private void OnCreateButtonPressed(object? s, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.UpdateFunction();
                UpdateGrid();
            }
        }
        //Функция отрисовки самих осей
        private void DrawAxis(
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            Color input_color, string label,
            double centerX, double centerY, double cellSize)
        {   
            if (DataContext is not MainWindowViewModel vm) return;
            Point start = new Point(0,0), end = new Point(0,0);
            var h = 100;
            double step = (double)(2 * CubeSize) / h;
            var color = new SolidColorBrush(input_color);
            
            var cameraPos = CalculateCameraPosition();
            var middleP = new Point3D((x2 - x1) / 2, (y2 - y1) / 2, (z2 - z1) / 2);
            for (double i = -CubeSize; i < CubeSize - step; i += step)
            {
                double dist = 0;
                if (x1 != 0 && x2 != 0)
                {
                    middleP = new Point3D((i + step + i) / 2, (y2 - y1) / 2, (z2 - z1) / 2);
                    start = ProjectTo2D(i, y1, z1, cellSize);
                    end = ProjectTo2D(i + step, y2, z2, cellSize);
                }
                else if (y1 != 0 && y2 != 0)
                {
                    middleP = new Point3D((x2 - x1) / 2, (i + step + i) / 2, (z2 - z1) / 2);
                    start = ProjectTo2D(x1, i, z1, cellSize);
                    end = ProjectTo2D(x2, i + step, z2, cellSize);
                }
                else if (z1 != 0 && z2 != 0)
                {
                    middleP = new Point3D((x2 - x1) / 2, (y2 - y1) / 2, (i + step + i) / 2);
                    start = ProjectTo2D(x1, y1, i, cellSize);
                    end = ProjectTo2D(x2, y2, i + step, cellSize);
                }
                else
                    return;
                dist = Math.Sqrt(Math.Pow(cameraPos.X - middleP.X, 2)
                    + Math.Pow(cameraPos.Y - middleP.Y, 2)
                    + Math.Pow(cameraPos.Z - middleP.Z, 2));
                var line = new Line
                {
                    StartPoint = new Point(start.X + centerX, start.Y + centerY),
                    EndPoint = new Point(end.X + centerX, end.Y + centerY),
                    Stroke = color,
                    StrokeThickness = 2
                };
                //canvas.Children.Add(line);
                var add_line = new Polygons(line, dist, new Point3D(0, 0, 0));
                polygons.Add(add_line);
            }
            
        }
        private void DrawAxisLabel(
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            Color input_color, string label,
            double centerX, double centerY, double cellSize)
            {
                Point start = new Point(0,0), end = new Point(0,0);
                var color = new SolidColorBrush(input_color);

                start = ProjectTo2D(x1, y1, z1, cellSize);
                end = ProjectTo2D(x2, y2, z2, cellSize);

                var textBlock = CreateLabel(label, color, 16);
                textBlock.FontWeight = FontWeight.Bold;

                canvas.Children.Add(textBlock);
                Canvas.SetLeft(textBlock, end.X + centerX + (label == "X" ? 10 : 0));
                Canvas.SetTop(textBlock, end.Y + centerY - (label == "Z" ? 20 : 0));
            }
        //Функция для отрисовки подписей к самим осям (X,Y,Z)
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
        //Обновляем координаты мыши в интерфейсе
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
                        // Если мы на оси X, пробуем найти Z (вертикальная координата)
                        zCoord = GetZValueOnXAxis(mouseX, mouseY, centerX, centerY, value);
                        break;
                    case "Y":
                        yCoord = formattedValue;
                        // Если мы на оси Y, пробуем найти Z (вертикальная координата)
                        zCoord = GetZValueOnYAxis(mouseX, mouseY, centerX, centerY, value);
                        break;
                    case "Z":
                        zCoord = formattedValue;
                        break;
                }
            }

            vm.GridCoordinates = $"X: {xCoord}, Y: {yCoord}, Z: {zCoord}";
        }
        //Функция для получения координаты z по оси X
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
        //Функция для получения координаты Z по оси Y
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
        //Получение значения осей
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
         //Функция поиска ближайших осей
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
        //Функция определяющая расстояние между точкой и линией
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
        //Математический интерполятор
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
        // Команды для кнопок (масштабирование, вращение)
        public void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            CubeSize = Math.Clamp(CubeSize + 1, 1, 20);
            UpdateGrid();
        }

        public void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            CubeSize = Math.Clamp(CubeSize - 1, 1, 20);
            UpdateGrid();
        }

        public void RotateRight_Click(object sender, RoutedEventArgs e)
        {
            _angleY += 0.1;
            UpdateGrid();
        }

        public void RotateLeft_Click(object sender, RoutedEventArgs e)
        {
            _angleY -= 0.1;
            UpdateGrid();
        }
        public void RotateTop_Click(object sender, RoutedEventArgs e)
        {
            _angleX += 0.1;
            UpdateGrid();
        }

        public void RotateBottom_Click(object sender, RoutedEventArgs e)
        {
            _angleX -= 0.1;
            UpdateGrid();
        }
        //Отрисовка объектов при загрузке окна
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            UpdateGrid();
        }
        //Отрисовка объектов при изменениях размеров окна
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGrid();
        }
        //Обработка нажатия кнопки мыши
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            _lastMousePosition = e.GetPosition(this);
            _isDragging = true;
            _isMoved = true;
            UpdateGrid();
        }
        //Обработка отжатия кнопки мыши
        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            _isMoved = false;
            UpdateGrid();
        }
        //Обработка движения мыши
        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isDragging) return;
            canvas.Children.Clear();

            var currentPosition = e.GetPosition(this);
            var delta = currentPosition - _lastMousePosition;
            if (canvas.IsPointerOver)
            {
                _angleY -= delta.X * 0.01;
                _angleX += delta.Y * 0.01;
                _angleX = Math.Clamp(_angleX, -Math.PI/2 + 0.1, Math.PI/2 - 0.1);
            }
            _lastMousePosition = currentPosition;
            UpdateGrid();
        }
        //Обработка движения колёсика мыши
        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            _isMoved = true;
            if (!_isWheeling)
            {
                _isWheeling = true;
            }

            // Сброс таймера завершения
            if (WheelTimer != null)
            {
                WheelTimer.Stop();
            }
            else
            {
                WheelTimer = new DispatcherTimer 
                { 
                    Interval = TimeSpan.FromMilliseconds(400) 
                };
                WheelTimer.Tick += OnWheelEnded;
            }
            
            WheelTimer.Start();
            
            // Оригинальный код для масштабирования
            var zoomFactor = e.Delta.Y > 0 ? 0.9 : 1.1;
            GridSize *= zoomFactor;
            GridSize = Math.Clamp(GridSize, 0.01, 50000);
            UpdateGrid();
        }
        private void OnWheelEnded(object sender, EventArgs e)
        {
            WheelTimer.Stop();
            _isWheeling = false;
            
            // Действие после завершения вращения
            _isMoved = false;
            UpdateGrid(); 
            
            // Очистка таймера
            WheelTimer.Tick -= OnWheelEnded;
            WheelTimer = null;
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                if (!_scaleCube) 
                    _scaleCube = true;
                else
                    _scaleCube = false;
            }
        }
        //Функция для расчёта динамического шага при масштабировании
        private double DynStepCalc(double gridSize)
        {
            // Определяем текущий "уровень масштаба" (0, 1, 2, 0, 1, 2...)
            int scaleLevel = (int)(Math.Log(gridSize, 2)) % 2;
            
            // Возвращаем шаг: 2^1=2, 2^2=4, 2^3=8, затем снова 2...
            return Math.Pow(2, scaleLevel + 1);
        }
        //Функция для отрисовки каркаса сетки
        private void DrawCubeFrame(double centerX, double centerY, double cellSize)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            Color frameColor = vm.ColorsScene.ColorCube;
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
        //Функция для отрисовки линии (используется в отрисовке куба)
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
        //Функция для отрисовки сетки на XY
        private void DrawXYPlane(double centerX, double centerY, double gridStep, double cellSize)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            Color gridColor = vm.ColorsScene.ColorGrid;
            Brush colorLabel = new SolidColorBrush(gridColor);
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
                DrawLabel((x).ToString("0.##"), colorLabel, 
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
                DrawLabel(y.ToString("0.##"), colorLabel, labelPos, 
                centerX, centerY, horizontal: false);
            }

        }
        //Функция для отрисовки подписей на осях
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
        private double DistanceCalc(double x1, double x2, double x3, 
        double y1, double y2, double y3, 
        double z1, double z2, double z3, 
        Point3D cameraPos)
        {
            // Используем ближайшую точку треугольника к камере
            double dist1 = Math.Sqrt(Math.Pow(cameraPos.X - x1, 2) + 
                                    Math.Pow(cameraPos.Y - y1, 2) + 
                                    Math.Pow(cameraPos.Z - z1, 2));
            
            double dist2 = Math.Sqrt(Math.Pow(cameraPos.X - x2, 2) + 
                                    Math.Pow(cameraPos.Y - y2, 2) + 
                                    Math.Pow(cameraPos.Z - z2, 2));
            
            double dist3 = Math.Sqrt(Math.Pow(cameraPos.X - x3, 2) + 
                                    Math.Pow(cameraPos.Y - y3, 2) + 
                                    Math.Pow(cameraPos.Z - z3, 2));
            
            return Math.Min(Math.Min(dist1, dist2), dist3);
        }
        private Point3D MiddlePointCalc(double x1, double x2, double x3, 
        double y1, double y2, double y3, 
        double z1, double z2, double z3)
        {
            double x = (x1 + x2 + x3) / 3;
            double y = (y1 + y2 + y3) / 3;
            double z = (z1 + z2 + z3) / 3;
            return new Point3D(x, y, z);
        }
        private Point3D CalculateCameraPosition()
        {
            //double scaleFactor = CubeSize / GridSize;
            // Предполагаем расстояние до камеры 1 единицу
            double distance = 10;

            
            // Вычисляем позицию камеры в сферических координатах
            double x = distance * Math.Cos(_angleX) * Math.Sin(_angleY);
            double y = distance * Math.Cos(_angleX) * Math.Cos(_angleY);
            double z = distance * Math.Sin(_angleX);

            return new Point3D(x, -y, z);
        }
        //Функция для отрисовки графика двумерной функции z(x,y)
        private void DrawFunctionSurface(double centerX, double centerY, double cellSize, FunctionModel f)
        {
            var CameraPos = CalculateCameraPosition();
            if (!(DataContext is MainWindowViewModel vm)) return;
            double step;
            if (!_isMoved)
                step = GridSize / f.StepSize;
            else
                step = GridSize / 10;
            //byte opacity = (byte)(vm.SurfaceOpacity * 255);
            Color surfaceColor;
            double discontinuityThreshold = GridSize * 2;
            // Масштабируем размер функции в соответствии с GridSize
            double scaleFactor = CubeSize / GridSize;
            
            for (double x = -GridSize; x < GridSize; x += step)
            {
                for (double y = -GridSize; y < GridSize; y += step)
                {
                    var color_h = f.StepColor;
                    surfaceColor = f.ColorStart;

                    var Zh = 2 * GridSize / color_h;
                    // Координаты в масштабе функции
                    double x1 = x * scaleFactor;
                    double y1 = y * scaleFactor;
                    double x2 = (x + step) * scaleFactor;
                    double y2 = (y + step) * scaleFactor;

                    double z1 = SafeCalculate(vm, x, y, discontinuityThreshold, step);
                    double z2 = SafeCalculate(vm, x + step, y, discontinuityThreshold, step);
                    double z3 = SafeCalculate(vm, x + step, y + step, discontinuityThreshold, step);
                    double z4 = SafeCalculate(vm, x, y + step, discontinuityThreshold, step);

                    // Пропускаем полигон, если обнаружен разрыв
                    if (double.IsInfinity(z1) || double.IsNaN(z1)) continue;
                    if (double.IsInfinity(z2) || double.IsNaN(z2)) continue;
                    if (double.IsInfinity(z3) || double.IsNaN(z3)) continue;
                    if (double.IsInfinity(z4) || double.IsNaN(z4)) continue;

                    
                    // Вычисляем z для каждой точки
                    z1 = Math.Clamp(z1, -GridSize, GridSize) * scaleFactor;
                    z2 = Math.Clamp(z2, -GridSize, GridSize) * scaleFactor;
                    z3 = Math.Clamp(z3, -GridSize, GridSize) * scaleFactor;
                    z4 = Math.Clamp(z4, -GridSize, GridSize) * scaleFactor;

                    var color1 = GradientColorCalculate(z1 / scaleFactor, Zh, color_h, f);
                    var color2 = GradientColorCalculate(z2 / scaleFactor, Zh, color_h, f);
                    var color3 = GradientColorCalculate(z3 / scaleFactor, Zh, color_h, f);
                    var color4 = GradientColorCalculate(z4 / scaleFactor, Zh, color_h, f);
                    
                    // Проецируем точки в 2D
                    var p1 = ProjectTo2D(x1, y1, z1, cellSize);
                    var p2 = ProjectTo2D(x2, y1, z2, cellSize);
                    var p3 = ProjectTo2D(x2, y2, z3, cellSize);
                    var p4 = ProjectTo2D(x1, y2, z4, cellSize);

                    double tr1_dist = DistanceCalc(x1, x2, x2,
                    y1, y1, y2, z1, z2, z3, CameraPos);
                    double tr2_dist = DistanceCalc(x1, x2, x1,
                    y1, y2, y2, z1, z3, z4, CameraPos);

                    var tr1_middleP = MiddlePointCalc(x1, x2, x2, 
                    y1, y1, y2, z1, z2, z3);
                    var tr2_middleP = MiddlePointCalc(x1, x2, x1,
                    y1, y2, y2, z1, z3, z4);
                    //Обработка разрывов
                    //if (!ShouldSkipPolygon(p1, p2, p3, p4, cellSize * 10))
                    {
                        var triangle1 = new Polygon
                        {
                            Points = new Points {
                                new Point(p1.X + centerX, p1.Y + centerY),
                                new Point(p2.X + centerX, p2.Y + centerY),
                                new Point(p3.X + centerX, p3.Y + centerY)
                            },
                            // Fill = new SolidColorBrush(surfaceColor),
                            Fill = new RadialGradientBrush
                            {
                                GradientStops =
                                {
                                    new GradientStop { Color = color1, Offset = 0 },
                                    new GradientStop { Color = color2, Offset = 0.5 },
                                    new GradientStop { Color = color3, Offset = 1 }
                                },
                                Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                                GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                                Radius = 1
                            },
                            Stroke = new SolidColorBrush(color1),
                            Opacity = double.IsInfinity(z1) || double.IsInfinity(z2) || double.IsInfinity(z3) ? 0.5 : 1
                        };

                        var triangle2 = new Polygon
                        {
                            Points = new Points {
                                new Point(p1.X + centerX, p1.Y + centerY),
                                new Point(p3.X + centerX, p3.Y + centerY),
                                new Point(p4.X + centerX, p4.Y + centerY)
                            },
                            //Fill = new SolidColorBrush(surfaceColor),
                            Fill = new RadialGradientBrush
                            {
                                GradientStops =
                                {
                                    new GradientStop { Color = color1, Offset = 0 },
                                    new GradientStop { Color = color3, Offset = 0.5 },
                                    new GradientStop { Color = color4, Offset = 1 }
                                },
                                Center = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                                GradientOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                                Radius = 1
                            },
                            Stroke = new SolidColorBrush(color1),
                            Opacity = double.IsInfinity(z1) || double.IsInfinity(z3) || double.IsInfinity(z4) ? 0.5 : 1
                        };
                        var triangle_poly1 = new Polygons(triangle1, tr1_dist, tr1_middleP);
                        var triangle_poly2 = new Polygons(triangle2, tr2_dist, tr2_middleP);
                        polygons.Add(triangle_poly1);
                        polygons.Add(triangle_poly2);
                        //canvas.Children.Add(triangle1);
                        //canvas.Children.Add(triangle2);
                    }
                }
            }
        }
        private Color GradientColorCalculate(double z, double Zh, double color_h, FunctionModel f)
        {
            if (!(DataContext is MainWindowViewModel vm)) return new Color(0,0,0,0);
            var colorStart = f.ColorStart;
            var colorEnd = f.ColorEnd;
            var color_R = colorStart.R;
            var color_G = colorStart.G;
            var color_B = colorStart.B;

            var color_Rh = (byte) (Math.Abs(colorStart.R - colorEnd.R)/color_h);
            var color_Gh = (byte) (Math.Abs(colorStart.G - colorEnd.G)/color_h);
            var color_Bh = (byte) (Math.Abs(colorStart.B - colorEnd.B)/color_h);

            byte count = 0;
            double z_start = -GridSize;
            if (z > 4.9)
            {
                Console.WriteLine();
            }

            while (z_start+Zh <= z)
            {
                z_start += Zh;
                if (color_R > colorEnd.R) color_R -= color_Rh;
                else color_R += color_Rh;
                if (color_G > colorEnd.G) color_G -= color_Gh;
                else color_G += color_Gh;
                if (color_B > colorEnd.B) color_B -= color_Bh;
                else color_B += color_Bh;
            }
            return new Color(colorStart.A, color_R, color_G, color_B);

        }
        //Безопасный расчёт значения функции с учётом разрывов второго рода
        private double SafeCalculate(MainWindowViewModel vm, double x, double y, double threshold, double step)
        {
            try
            {   
                double result = vm.CalculateFunction(x, y);
                double prev_result = vm.CalculateFunction(x, y - step);
                if (double.IsNaN(result))
                        return double.NaN;
                if (double.IsInfinity(result) || Math.Abs(result) > threshold)
                    return double.PositiveInfinity;
                    
                return result;
            }
            catch
            {
                return double.PositiveInfinity;
            }
        }
        //Функция для расчёта расстояния между точками
        private double DistanceBetween(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        //Структура 3D точки
        public struct Point3D
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
        //Статичная функция (не используется)
        private double Function(double x, double y)
        {
            return Math.Exp(x) + Math.Exp(y);
        }
        //Функция для преобразования 3D в 2D
        private Point ProjectTo2D(double x, double y, double z, double cellSize)
        {
            // Изометрическая проекция с Z как вертикальной осью (направленной вверх)
            double x2d = x * Math.Cos(_angleY) + y * Math.Sin(_angleY);
            double y2d = x * Math.Sin(_angleX) * Math.Sin(_angleY) - 
                        y * Math.Sin(_angleX) * Math.Cos(_angleY) - 
                        z * Math.Cos(_angleX);
            
            return new Point(x2d * cellSize, y2d * cellSize);
        }
        private async void SaveToPng_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить как PNG",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "PNG файлы", Extensions = new List<string> { "png" } },
                    new FileDialogFilter { Name = "Все файлы", Extensions = new List<string> { "*" } }
                },
                DefaultExtension = "png"
            };

            var result = await saveFileDialog.ShowAsync(this);
            if (result != null)
            {
                try
                {
                    var size = new Size(canvas.Bounds.Width, canvas.Bounds.Height);
                    using (var renderTarget = new RenderTargetBitmap(new PixelSize((int)size.Width, (int)size.Height)))
                    {
                        canvas.Measure(size);
                        canvas.Arrange(new Rect(size));
                        renderTarget.Render(canvas);
                        renderTarget.Save(result);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении PNG: {ex.Message}");
                }
            }
        }
        private JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ColorJsonConverter(), new ColorSceneJsonConverter() }
        };

        private async void SaveToJson_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm) return;
            
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить как JSON",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "JSON файлы", Extensions = new List<string> { "json" } },
                    new FileDialogFilter { Name = "Все файлы", Extensions = new List<string> { "*" } }
                },
                DefaultExtension = "json"
            };

            var result = await saveFileDialog.ShowAsync(this);
            if (result != null)
            {
                try
                {
                    var data = new FunctionData
                    {
                        Functions = new List<FunctionModel>(vm.Functions),
                        ColorsScene = vm.ColorsScene,
                        ShowAxes = vm.ShowAxes,
                        ShowLabels = vm.ShowLabels,
                        ShowCube = vm.ShowCube,
                        ShowGrid = vm.ShowGrid,
                        ShowGraphic = vm.ShowGraphic,
                        GridSize = GridSize,
                        AngleX = _angleX,
                        AngleY = _angleY
                    };

                    var json = JsonSerializer.Serialize(data, jsonOptions);
                    await File.WriteAllTextAsync(result, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении JSON: {ex.Message}");
                }
            }
        }

        private async void LoadFromJson_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Загрузить из JSON",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "JSON файлы", Extensions = new List<string> { "json" } },
                    new FileDialogFilter { Name = "Все файлы", Extensions = new List<string> { "*" } }
                },
                AllowMultiple = false
            };

            var result = await openFileDialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(result[0]);
                    var data = JsonSerializer.Deserialize<FunctionData>(json, jsonOptions);
                    
                    if (DataContext is MainWindowViewModel vm)
                    {
                        vm.Functions = new ObservableCollection<FunctionModel>(data.Functions);
                        vm.ColorsScene = data.ColorsScene;
                        vm.ShowAxes = data.ShowAxes;
                        vm.ShowLabels = data.ShowLabels;
                        vm.ShowCube = data.ShowCube;
                        vm.ShowGrid = data.ShowGrid;
                        vm.ShowGraphic = data.ShowGraphic;
                        
                        GridSize = data.GridSize;
                        _angleX = data.AngleX;
                        _angleY = data.AngleY;
                        
                        vm.SelectedFunction = vm.Functions.FirstOrDefault();
                        UpdateGrid();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке JSON: {ex.Message}");
                }
            }
        }
    }
}