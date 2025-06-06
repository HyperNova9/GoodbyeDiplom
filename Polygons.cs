using System.Reflection;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using ReactiveUI;
using static GoodbyeDiplom.Views.MainWindow;
namespace GoodbyeDiplom.ViewModels;

public class Polygons : ViewModelBase
{
    public Shape triangle;
    public double distance = 0;
    public Point3D middlePoint;
    public Polygons(Shape triangle, double distance, Point3D middlePoint)
    {
        this.triangle = triangle;
        this.distance = distance;
        this.middlePoint = middlePoint;
    }

}