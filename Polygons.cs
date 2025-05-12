using System.Reflection;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ReactiveUI;
using static GoodbyeDiplom.Views.MainWindow;
namespace GoodbyeDiplom.ViewModels;

public class Polygons : ViewModelBase
{
    public Polygon triangle;
    public double distance = 0;
    public Point3D middlePoint;
    public Polygons(Polygon triangle, double distance)
    {
        this.triangle = triangle;
        this.distance = distance;
    }

}