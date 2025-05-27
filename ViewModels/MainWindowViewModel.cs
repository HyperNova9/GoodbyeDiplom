#pragma warning disable CS8618
using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using GoodbyeDiplom.Views;

namespace GoodbyeDiplom.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private string _gridCoordinates = "X: —, Y: —, Z: —";
    private string _functionExpression = "sin(x) + cos(y)";
    private readonly MathExpressionParser _parser = new MathExpressionParser();
    public Func<double, double, double> _function;
    public ColorScene _colorsScene;

    private Color _startColor = Color.FromArgb(160, 0, 0, 0);
    private Color _endColor = Color.FromArgb(160, 255, 255, 255);
    private double _stepSize = 10;
    private double _stepColor = 10;
    private bool _showAxes = true;
    private bool _showLabels = true;
    private bool _showCube = true;
    private bool _showGrid = true;
    private bool _showGraphic = true;
    private bool _isDarkMode= false;
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
                StartColor = value.ColorStart;
                EndColor = value.ColorEnd;
                StepSize = value.StepSize;
                StepColor = value.StepColor;
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
    public MainWindowViewModel()
    {
        // Инициализация команд
        AddFunctionCommand = ReactiveCommand.Create(AddFunction);
        RemoveFunctionCommand = ReactiveCommand.Create(RemoveFunction);
        // Добавим пример функции по умолчанию
        _functions.Add(new FunctionModel(
            "Пример 1", 
            "sin(x) + cos(y)", 
            StartColor,
            EndColor, 
            10,
            10,
            true));
        SelectedFunction = Functions.FirstOrDefault();
        ColorsScene = new ColorScene(Colors.Red, Colors.Blue, Colors.Green, 
        Colors.Black, Colors.White, Color.FromArgb(255, 150, 150, 150));
        UpdateFunction();

    }
    private void AddFunction()
    {
        var newFunc = new FunctionModel(
            $"Функция {Functions.Count + 1}",
            "0",
            Color.FromArgb(255, (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255))),
            Color.FromArgb(255, (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255)), 
            (byte)(new Random().Next(255))),
            10,
            10,
            false);
        
        Functions.Add(newFunc);
        SelectedFunction = Functions.FirstOrDefault();
    }
    
    private void RemoveFunction()
    {
        if (SelectedFunction != null)
        {
            Functions.Remove(SelectedFunction);
            SelectedFunction = null;
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
    
    public Color StartColor
    {
        get => _startColor;
        set => this.RaiseAndSetIfChanged(ref _startColor, value);
    }
    public Color EndColor
    {
        get => _endColor;
        set => this.RaiseAndSetIfChanged(ref _endColor, value);
    }
    public double StepSize
    {
        get => _stepSize;
        set => this.RaiseAndSetIfChanged(ref _stepSize, value);
    }
    public double StepColor
    {
        get => _stepColor;
        set => this.RaiseAndSetIfChanged(ref _stepColor, value);
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
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set => this.RaiseAndSetIfChanged(ref _isDarkMode, value);
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
        StartColor = color;
    }
}