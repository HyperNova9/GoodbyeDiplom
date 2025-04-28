#pragma warning disable CS8618
using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;

namespace GoodbyeDiplom.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private string _gridCoordinates = "X: —, Y: —, Z: —";
    private string _functionExpression = "sin(x) + cos(y)";
    private readonly MathExpressionParser _parser = new MathExpressionParser();
    public Func<double, double, double> _function;
    public ColorScene _colorsScene;

    private Color _surfaceColor = Color.FromArgb(160, 0, 89, 179);
    private double _stepSize = 10;
    private bool _showAxes = true;
    private bool _showLabels = true;
    private bool _showCube = true;
    private bool _showGrid = true;
    private bool _showGraphic = true;
    private ObservableCollection<FunctionModel> _functions = new();
    private FunctionModel _selectedFunction;
    public ObservableCollection<FunctionModel> Functions
    {
        get => _functions;
        set => this.RaiseAndSetIfChanged(ref _functions, value);
    }
    public FunctionModel SelectedFunction
    {
        get => _selectedFunction;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedFunction, value);
            if (value != null)
            {
                FunctionExpression = value.Expression;
                SurfaceColor = value.Color;
                StepSize = value.StepSize;
                UpdateFunction();
            }
        }
    }
    public ColorScene ColorsScene
    {
        get => _colorsScene;
        set
        {
            this.RaiseAndSetIfChanged(ref _colorsScene, value);
        }
    }
    public bool HasSelectedFunction => SelectedFunction != null;
    
    public ReactiveCommand<Unit, Unit> AddFunctionCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveFunctionCommand { get; }
    public ReactiveCommand<Unit, Unit> ApplyFunctionCommand { get; }
    public MainWindowViewModel()
    {
        // Инициализация команд
        AddFunctionCommand = ReactiveCommand.Create(AddFunction);
        RemoveFunctionCommand = ReactiveCommand.Create(RemoveFunction);
        ApplyFunctionCommand = ReactiveCommand.Create(ApplyFunction);
        // Подписка на изменения
        this.WhenAnyValue(x => x.SelectedFunction)
            .Subscribe(_ => UpdateFunctionAndGrid());
        // Добавим пример функции по умолчанию
        _functions.Add(new FunctionModel(
            "Пример 1", 
            "sin(x) + cos(y)", 
            Color.FromArgb(160, 0, 89, 179), 
            10));
        SelectedFunction = Functions.FirstOrDefault();
        ColorsScene = new ColorScene(Colors.Red, Colors.Blue, Colors.Green, 
        Colors.Black, Colors.White, Color.FromArgb(80, 150, 150, 150));
        UpdateFunction();

    }
    private void UpdateFunctionAndGrid()
    {
        if (SelectedFunction != null)
        {
            FunctionExpression = SelectedFunction.Expression;
            SurfaceColor = SelectedFunction.Color;
            StepSize = SelectedFunction.StepSize;
            UpdateFunction();
            UpdateData?.Invoke(this, StepSize); // Уведомляем об изменении
        }
    }
    private void AddFunction()
    {
        var newFunc = new FunctionModel(
            $"Функция {Functions.Count + 1}",
            "0",
            Color.FromArgb(160, (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255))),
            10);
        
        Functions.Add(newFunc);
        SelectedFunction = Functions.FirstOrDefault();
    }
    
    private void RemoveFunction()
    {
        if (SelectedFunction != null)
        {
            Functions.Remove(SelectedFunction);
            SelectedFunction = null;
            UpdateData?.Invoke(this, StepSize); // Уведомляем об изменении
        }
    }
    
    private void ApplyFunction()
    {
        if (SelectedFunction != null)
        {
            SelectedFunction.Expression = FunctionExpression;
            SelectedFunction.Color = SurfaceColor;
            SelectedFunction.StepSize = StepSize;
            UpdateFunction();
            UpdateData?.Invoke(this, StepSize); // Уведомляем об изменении
        }
    }
    public event EventHandler<double> UpdateData;
    public string GridCoordinates
    {
        get => _gridCoordinates;
        set => this.RaiseAndSetIfChanged(ref _gridCoordinates, value);
    }
    
    public string FunctionExpression
    {
        get => _functionExpression;
        set => this.RaiseAndSetIfChanged(ref _functionExpression, value);
    }
    
    public Color SurfaceColor
    {
        get => _surfaceColor;
        set => this.RaiseAndSetIfChanged(ref _surfaceColor, value);
    }
    
    public double StepSize
    {
        get => _stepSize;
        set => this.RaiseAndSetIfChanged(ref _stepSize, value);
    }
    
    public bool ShowAxes
    {
        get => _showAxes;
        set => this.RaiseAndSetIfChanged(ref _showAxes, value);
    }
    
    public bool ShowLabels
    {
        get => _showLabels;
        set => this.RaiseAndSetIfChanged(ref _showLabels, value);
    }
    
    public bool ShowCube
    {
        get => _showCube;
        set => this.RaiseAndSetIfChanged(ref _showCube, value);
    }
    
    public bool ShowGrid
    {
        get => _showGrid;
        set
        {
            this.RaiseAndSetIfChanged(ref _showGrid, value);
        }
    }
    public bool ShowGraphic
    {
        get => _showGraphic;
        set
        {
            this.RaiseAndSetIfChanged(ref _showGraphic, value);
        }
    }
    public double CalculateFunction(double x, double y) => _function?.Invoke(x, y) ?? 0;
    
    public void UpdateFunction()
    {
        try
        {
            _function = _parser.Parse(FunctionExpression);
        }
        catch (Exception ex)
        {
            _function = (x, y) => 0;
            Console.WriteLine($"Ошибка парсинга: {ex.Message}");
        }
    }
    public void UpdateColor(Color color)
    {
        SurfaceColor = color;
    }
}