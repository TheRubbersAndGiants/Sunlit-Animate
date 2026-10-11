using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SunlitAnimate.Models;

namespace SunlitAnimate.Controls;

public sealed class DrawingSurface : Control
{
    private StrokeData? _activeStroke;
    private readonly Stack<List<StrokeData>> _undo = new();
    private readonly Stack<List<StrokeData>> _redo = new();

    public List<StrokeData> Strokes { get; private set; } = [];
    public string BrushColor { get; set; } = "#F4F0FF";
    public double BrushThickness { get; set; } = 4;
    public bool IsEraser { get; set; }

    public event EventHandler? StrokesChanged;

    public DrawingSurface()
    {
        Focusable = true;
        ClipToBounds = true;
    }

    public void LoadStrokes(IEnumerable<StrokeData> strokes)
    {
        Strokes = strokes.Select(CloneStroke).ToList();
        _undo.Clear();
        _redo.Clear();
        InvalidateVisual();
    }

    public void ClearDrawing()
    {
        if (Strokes.Count == 0)
            return;
        SaveUndoState();
        Strokes.Clear();
        _activeStroke = null;
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool Undo()
    {
        if (_undo.Count == 0)
            return false;
        _redo.Push(Strokes.Select(CloneStroke).ToList());
        Strokes = _undo.Pop();
        _activeStroke = null;
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public bool Redo()
    {
        if (_redo.Count == 0)
            return false;
        _undo.Push(Strokes.Select(CloneStroke).ToList());
        Strokes = _redo.Pop();
        _activeStroke = null;
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.DrawRectangle(Brushes.White, null, new Rect(Bounds.Size));

        foreach (var stroke in Strokes)
        {
            if (stroke.Points.Count == 0)
                continue;

            var color = ParseColor(stroke.Color);
            var pen = new Pen(new SolidColorBrush(color), stroke.Thickness,
                lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

            if (stroke.Points.Count == 1)
            {
                var point = ToPoint(stroke.Points[0]);
                context.DrawEllipse(new SolidColorBrush(color), null, point, stroke.Thickness / 2, stroke.Thickness / 2);
                continue;
            }

            for (var i = 1; i < stroke.Points.Count; i++)
                context.DrawLine(pen, ToPoint(stroke.Points[i - 1]), ToPoint(stroke.Points[i]));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        SaveUndoState();
        _redo.Clear();
        var position = e.GetPosition(this);
        _activeStroke = new StrokeData
        {
            Color = IsEraser ? "#FFFFFF" : BrushColor,
            Thickness = IsEraser ? Math.Max(12, BrushThickness * 3) : BrushThickness,
            Points = [new PointData { X = position.X, Y = position.Y }]
        };
        Strokes.Add(_activeStroke);
        e.Pointer.Capture(this);
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_activeStroke is null || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var position = e.GetPosition(this);
        _activeStroke.Points.Add(new PointData { X = position.X, Y = position.Y });
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_activeStroke is null)
            return;

        _activeStroke = null;
        e.Pointer.Capture(null);
        InvalidateVisual();
        StrokesChanged?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void SaveUndoState() => _undo.Push(Strokes.Select(CloneStroke).ToList());

    private static StrokeData CloneStroke(StrokeData stroke) => new()
    {
        Color = stroke.Color,
        Thickness = stroke.Thickness,
        Points = stroke.Points.Select(p => new PointData { X = p.X, Y = p.Y }).ToList()
    };

    private static Point ToPoint(PointData point) => new(point.X, point.Y);

    private static Color ParseColor(string value) =>
        Color.TryParse(value, out var color) ? color : Colors.Black;
}
