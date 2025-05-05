using Avalonia.Media;
using ReactiveUI;

namespace GoodbyeDiplom.ViewModels;

public class FunctionModel : ViewModelBase
{
    private string _name;
    private string _expression;
    private Color _colorStart;
    private Color _colorEnd;
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

    public Color ColorStart
    {
        get => _colorStart;
        set => this.RaiseAndSetIfChanged(ref _colorStart, value);
    }
    public Color ColorEnd
    {
        get => _colorEnd;
        set => this.RaiseAndSetIfChanged(ref _colorEnd, value);
    }
    public double StepSize
    {
        get => _stepSize;
        set => this.RaiseAndSetIfChanged(ref _stepSize, value);
    }

    public FunctionModel(string name, string expression, Color colorStart, Color colorEnd, double stepSize)
    {
        Name = name;
        Expression = expression;
        ColorStart = colorStart;
        ColorEnd = colorEnd;
        StepSize = stepSize;
    }
}