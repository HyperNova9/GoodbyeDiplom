#pragma warning disable CS8618
using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace GoodbyeDiplom.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private string _gridCoordinates = "X: —, Y: —, Z: —";
    private string _functionExpression = "sin(x) + cos(y)";
    private readonly MathExpressionParser _parser = new MathExpressionParser();
    public Func<double, double, double> _function;
    
    private double _surfaceOpacity = 0.8;
    private Color _surfaceColor = Color.FromArgb(160, 0, 89, 179);
    private double _stepSize = 10;
    private bool _showAxes = true;
    private bool _showLabels = true;
    private bool _showCube = true;
    private bool _showGrid = true;
    public MainWindowViewModel()
    {
        this.WhenAnyValue(x => x.StepSize).Subscribe(x => UpdateData?.Invoke(this, x));
        UpdateFunction();
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
    
    public double SurfaceOpacity
    {
        get => _surfaceOpacity;
        set => this.RaiseAndSetIfChanged(ref _surfaceOpacity, value);
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