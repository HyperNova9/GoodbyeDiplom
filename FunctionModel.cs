using Avalonia.Media;
using ReactiveUI;

namespace GoodbyeDiplom.ViewModels;

public class FunctionModel : ViewModelBase
{
    private string _name;
    private string _expression;
    private Color _color;
    private double _stepSize;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Expression
    {
        get => _expression;
        set => this.RaiseAndSetIfChanged(ref _expression, value);
    }

    public Color Color
    {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }

    public double StepSize
    {
        get => _stepSize;
        set => this.RaiseAndSetIfChanged(ref _stepSize, value);
    }

    public FunctionModel(string name, string expression, Color color, double stepSize)
    {
        Name = name;
        Expression = expression;
        Color = color;
        StepSize = stepSize;
    }
}