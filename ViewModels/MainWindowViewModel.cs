// MainWindowViewModel.cs
using System;
using ReactiveUI;
using System.Reactive;

namespace GoodbyeDiplom.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    private string _gridCoordinates = "X: —, Y: —, Z: —";
    private string _functionExpression = "sin(x) + cos(y)";
    private readonly MathExpressionParser _parser = new MathExpressionParser();
    private Func<double, double, double> _function;
    
    public MainWindowViewModel()
    {
        // Инициализация функции при создании
        UpdateFunction();
    }
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
    
    public ReactiveCommand<Unit, Unit> UpdateFunctionCommand { get; }
    
    public double CalculateFunction(double x, double y) => _function?.Invoke(x, y) ?? 0;
    
    public void UpdateFunction()
    {
        try
        {
            _function = _parser.Parse(FunctionExpression);
        }
        catch (Exception ex)
        {
            // Можно добавить вывод ошибки в интерфейс
            _function = (x, y) => 0;
            Console.WriteLine($"Ошибка парсинга: {ex.Message}");
        }
    }
}