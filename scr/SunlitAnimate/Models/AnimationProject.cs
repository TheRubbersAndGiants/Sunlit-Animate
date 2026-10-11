using System.Text.Json.Serialization;

namespace SunlitAnimate.Models;

public sealed class AnimationProject
{
    public int FormatVersion { get; set; } = 1;
    public string Name { get; set; } = "Untitled Animation";
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public int FramesPerSecond { get; set; } = 12;
    public int CurrentFrame { get; set; }
    public List<AnimationFrame> Frames { get; set; } = [new AnimationFrame()];
}

public sealed class AnimationFrame
{
    public List<StrokeData> Strokes { get; set; } = [];
}

public sealed class StrokeData
{
    public string Color { get; set; } = "#F4F0FF";
    public double Thickness { get; set; } = 4;
    public List<PointData> Points { get; set; } = [];
}

public sealed class PointData
{
    public double X { get; set; }
    public double Y { get; set; }
}
