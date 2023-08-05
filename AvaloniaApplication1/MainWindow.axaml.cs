using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AvaloniaApplication1;

public readonly record struct P(double X, double Y, double Z);

public readonly record struct Edge(P Start, P End);

public partial class MainWindow : Window
{
    private static readonly Edge[] edges = new[]
    {
        // front
        new Edge(new P(-1, 1, 1), new P(1, 1, 1)),
        new Edge(new P(1, 1, 1), new P(1, -1, 1)),
        new Edge(new P(1, -1, 1), new P(-1, -1, 1)),
        new Edge(new P(-1, -1, 1), new P(-1, 1, 1)),

        // back
        new Edge(new P(-1, 1, -1), new P(1, 1, -1)),
        new Edge(new P(1, 1, -1), new P(1, -1, -1)),
        new Edge(new P(1, -1, -1), new P(-1, -1, -1)),
        new Edge(new P(-1, -1, -1), new P(-1, 1, -1)),

        // sides 
        new Edge(new P(-1, 1, 1), new P(-1, 1, -1)),
        new Edge(new P(1, 1, 1), new P(1, 1, -1)),
        new Edge(new P(1, -1, 1), new P(1, -1, -1)),
        new Edge(new P(-1, -1, 1), new P(-1, -1, -1)),
    };

    private double xAngle = 0;
    private double yAngle = 0;
    private double zAngle = 0;
    private double size = 100;
    private bool isPressed = false;
    private Point startPosition;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Draw();
    }

    protected override void OnResized(WindowResizedEventArgs e)
    {
        base.OnResized(e);

        Draw();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        size += e.Delta.Y;
        Draw();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        isPressed = true;
        startPosition = e.GetCurrentPoint(this).Position;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (isPressed)
        {
            var currentPosition = e.GetCurrentPoint(this).Position;
            yAngle = ((startPosition.X - currentPosition.X) / 10) % 360;
            xAngle = ((currentPosition.Y - startPosition.Y) / 10) % 360;

            Draw();

            startPosition = e.GetCurrentPoint(this).Position;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        isPressed = false;
        xAngle = 0;
        yAngle = 0;
        zAngle = 0;
    }

    private void Draw()
    {
        Canvas.Children.Clear();

        var w = Canvas.Bounds.Width;
        var h = Canvas.Bounds.Height;

        var zAngle = this.zAngle * Math.PI / 180;
        var yAngle = this.yAngle * Math.PI / 180;
        var xAngle = this.xAngle * Math.PI / 180;

        for (var index = 0; index < edges.Length; index++)
        {
            var (start, end) = edges[index];

            // z rotation
            {
                var sX = start.X * Math.Cos(zAngle) - start.Y * Math.Sin(zAngle);
                var sY = start.X * Math.Sin(zAngle) + start.Y * Math.Cos(zAngle);

                var eX = end.X * Math.Cos(zAngle) - end.Y * Math.Sin(zAngle);
                var eY = end.X * Math.Sin(zAngle) + end.Y * Math.Cos(zAngle);

                edges[index] = edges[index] with
                {
                    Start = start with { X = sX, Y = sY },
                    End = end with { X = eX, Y = eY },
                };
                (start, end) = edges[index];
            }

            // y rotation
            {
                var sX = start.X * Math.Cos(yAngle) + start.Z * Math.Sin(yAngle);
                var sZ = start.X * -Math.Sin(yAngle) + start.Z * Math.Cos(yAngle);

                var eX = end.X * Math.Cos(yAngle) + end.Z * Math.Sin(yAngle);
                var eZ = end.X * -Math.Sin(yAngle) + end.Z * Math.Cos(yAngle);

                edges[index] = edges[index] with
                {
                    Start = start with { X = sX, Z = sZ },
                    End = end with { X = eX, Z = eZ },
                };
                (start, end) = edges[index];
            }

            // x rotation
            {
                var sY = start.Y * Math.Cos(xAngle) - start.Z * Math.Sin(xAngle);
                var sZ = start.Y * Math.Sin(xAngle) + start.Z * Math.Cos(xAngle);

                var eY = end.Y * Math.Cos(xAngle) - end.Z * Math.Sin(xAngle);
                var eZ = end.Y * Math.Sin(xAngle) + end.Z * Math.Cos(xAngle);

                edges[index] = edges[index] with
                {
                    Start = start with { Z = sZ, Y = sY },
                    End = end with { Z = eZ, Y = eY },
                };
                (start, end) = edges[index];
            }

            var line = new Line
            {
                Stroke = index switch
                {
                    >= 0 and < 4 => Brushes.Red,
                    >= 4 and < 8 => Brushes.DarkRed,
                    >= 8 and < 12 => Brushes.IndianRed,
                    _ => Brushes.Black,
                },
                StrokeThickness = 5,
                StartPoint = new Point(Scale(start.X, w), Scale(start.Y, h)),
                EndPoint = new Point(Scale(end.X, w), Scale(end.Y, h)),
            };

            Canvas.Children.Add(line);
        }
    }

    private double Scale(double v, double max)
        => v * size + max / 2;
}