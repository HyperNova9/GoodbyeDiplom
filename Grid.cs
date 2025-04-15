using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
using System;

namespace GoodbyeDiplom.Views;

public class CoordinateGridControl : Control
{
    private const int GridSize = 10; // Размер сетки в клетках
    private const int CellSize = 40; // Размер клетки в пикселях
    private const int AxisWidth = 2; // Толщина осей
    private const int GridLineWidth = 1; // Толщина линий сетки
    private readonly Color AxisColor = Colors.Red; // Цвет осей
    private readonly Color GridColor = Colors.Gray; // Цвет сетки
    private readonly Color TextColor = Colors.Black; // Цвет текста

    // Угол поворота для псевдо3D эффекта
    private double angleX = 35 * Math.PI / 180;
    private double angleY = -45 * Math.PI / 180;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var centerX = Bounds.Width / 2;
        var centerY = Bounds.Height / 2;

        // Рисуем сетку в плоскости XZ (пол)
        for (int x = -GridSize; x <= GridSize; x++)
        {
            var p1 = ProjectTo2D(x, 0, -GridSize);
            var p2 = ProjectTo2D(x, 0, GridSize);
            context.DrawLine(new Pen(GridColor.ToUInt32(), GridLineWidth), 
                            new Point(p1.X + centerX, p1.Y + centerY), 
                            new Point(p2.X + centerX, p2.Y + centerY));

            // Подписи осей X
            if (x != 0 && Math.Abs(x) % 2 == 0)
            {
                var text = new FormattedText(
                    x.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    12,
                    new SolidColorBrush(TextColor));
                
                var labelPos = ProjectTo2D(x, 0, 0);
                context.DrawText(text, new Point(labelPos.X + centerX - text.Width / 2, 
                                                    labelPos.Y + centerY + 5));
            }
        }

        for (int z = -GridSize; z <= GridSize; z++)
        {
            var p1 = ProjectTo2D(-GridSize, 0, z);
            var p2 = ProjectTo2D(GridSize, 0, z);
            context.DrawLine(new Pen(GridColor.ToUInt32(), GridLineWidth), 
                            new Point(p1.X + centerX, p1.Y + centerY), 
                            new Point(p2.X + centerX, p2.Y + centerY));

            // Подписи осей Z
            if (z != 0 && Math.Abs(z) % 2 == 0)
            {
                var text = new FormattedText(
                    z.ToString(),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    12,
                    new SolidColorBrush(TextColor));
                
                var labelPos = ProjectTo2D(0, 0, z);
                context.DrawText(text, new Point(labelPos.X + centerX + 5, 
                                                    labelPos.Y + centerY - text.Height / 2));
            }
        }

        // Рисуем оси
        // Ось X (красная)
        var xAxisStart = ProjectTo2D(-GridSize, 0, 0);
        var xAxisEnd = ProjectTo2D(GridSize, 0, 0);
        context.DrawLine(new Pen(AxisColor.ToUInt32(), AxisWidth), 
                        new Point(xAxisStart.X + centerX, xAxisStart.Y + centerY), 
                        new Point(xAxisEnd.X + centerX, xAxisEnd.Y + centerY));

        // Ось Y (зеленая - вертикальная)
        var yAxisStart = ProjectTo2D(0, -GridSize, 0);
        var yAxisEnd = ProjectTo2D(0, GridSize, 0);
        context.DrawLine(new Pen(Colors.Green.ToUInt32(), AxisWidth), 
                        new Point(yAxisStart.X + centerX, yAxisStart.Y + centerY), 
                        new Point(yAxisEnd.X + centerX, yAxisEnd.Y + centerY));

        // Ось Z (синяя)
        var zAxisStart = ProjectTo2D(0, 0, -GridSize);
        var zAxisEnd = ProjectTo2D(0, 0, GridSize);
        context.DrawLine(new Pen(Colors.Blue.ToUInt32(), AxisWidth), 
                        new Point(zAxisStart.X + centerX, zAxisStart.Y + centerY), 
                        new Point(zAxisEnd.X + centerX, zAxisEnd.Y + centerY));

        // Подписи осей
        var xLabel = new FormattedText("X", 
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        Typeface.Default,
                                        14,
                                        new SolidColorBrush(AxisColor));
        context.DrawText(xLabel, new Point(xAxisEnd.X + centerX + 10, xAxisEnd.Y + centerY));

        var yLabel = new FormattedText("Y", 
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        Typeface.Default,
                                        14,
                                        new SolidColorBrush(Colors.Green));
        context.DrawText(yLabel, new Point(yAxisEnd.X + centerX, yAxisEnd.Y + centerY - 20));

        var zLabel = new FormattedText("Z", 
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        Typeface.Default,
                                        14,
                                        new SolidColorBrush(Colors.Blue));
        context.DrawText(zLabel, new Point(zAxisEnd.X + centerX, zAxisEnd.Y + centerY - 20));
    }

    // Проекция 3D точки в 2D с псевдо3D эффектом
    private Point ProjectTo2D(double x, double y, double z)
    {
        // Простая изометрическая проекция
        double x2d = x * Math.Cos(angleY) + z * Math.Sin(angleY);
        double y2d = x * Math.Sin(angleX) * Math.Sin(angleY) + 
                        y * Math.Cos(angleX) - 
                        z * Math.Sin(angleX) * Math.Cos(angleY);
        
        return new Point(x2d * CellSize, y2d * CellSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Задаем рекомендуемый размер для контроля
        return new Size(GridSize * CellSize * 2, GridSize * CellSize * 2);
    }
}
