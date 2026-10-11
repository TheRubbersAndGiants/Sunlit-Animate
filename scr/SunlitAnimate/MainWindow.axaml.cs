using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using SunlitAnimate.Models;

namespace SunlitAnimate;

public partial class MainWindow : Window
{
    private AnimationProject _project = new();
    private int _frameIndex;
    private bool _isEraser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public MainWindow()
    {
        InitializeComponent();
        CanvasSurface.StrokesChanged += (_, _) => StoreCurrentFrame();
        BrushSizeInput.ValueChanged += (_, _) => UpdateBrushSettings();
        ColorInput.TextChanged += (_, _) => UpdateBrushSettings();
        FpsInput.ValueChanged += (_, _) => _project.FramesPerSecond = Math.Clamp((int)(FpsInput.Value ?? 12), 1, 60);
        LoadCurrentFrame();
        UpdateFrameLabels();
    }

    private void UpdateBrushSettings()
    {
        CanvasSurface.BrushThickness = (double)(BrushSizeInput.Value ?? 4);
        CanvasSurface.BrushColor = ColorInput.Text ?? "#252027";
        CanvasSurface.IsEraser = _isEraser;
    }

    private void StoreCurrentFrame()
    {
        if (_project.Frames.Count == 0 || _frameIndex >= _project.Frames.Count)
            return;
        _project.Frames[_frameIndex].Strokes = CanvasSurface.Strokes;
    }

    private void LoadCurrentFrame()
    {
        if (_project.Frames.Count == 0)
            _project.Frames.Add(new AnimationFrame());
        _frameIndex = Math.Clamp(_frameIndex, 0, _project.Frames.Count - 1);
        CanvasSurface.LoadStrokes(_project.Frames[_frameIndex].Strokes);
        UpdateBrushSettings();
        UpdateFrameLabels();
    }

    private void UpdateFrameLabels()
    {
        FrameNumberText.Text = $"{_frameIndex + 1} / {_project.Frames.Count}";
        ProjectNameText.Text = _project.Name;
    }

    private void NewProject_Click(object? sender, RoutedEventArgs e)
    {
        _project = new AnimationProject();
        _frameIndex = 0;
        LoadCurrentFrame();
        StatusText.Text = "New project created";
    }

    private async void OpenProject_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Sunlit Animate project",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Sunlit Animate Project") { Patterns = ["*.sanp", "*.json"] }
            ]
        });

        if (files.Count == 0)
            return;

        try
        {
            await using var stream = await files[0].OpenReadAsync();
            var project = await JsonSerializer.DeserializeAsync<AnimationProject>(stream, JsonOptions);
            if (project is null || project.FormatVersion != 1 || project.Frames.Count == 0)
                throw new InvalidDataException("This project file is not supported.");

            _project = project;
            _frameIndex = Math.Clamp(_project.CurrentFrame, 0, _project.Frames.Count - 1);
            FpsInput.Value = Math.Clamp(_project.FramesPerSecond, 1, 60);
            LoadCurrentFrame();
            StatusText.Text = $"Opened {files[0].Name}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not open project: {ex.Message}";
        }
    }

    private async void SaveProject_Click(object? sender, RoutedEventArgs e)
    {
        StoreCurrentFrame();
        _project.CurrentFrame = _frameIndex;
        _project.FramesPerSecond = Math.Clamp((int)(FpsInput.Value ?? 12), 1, 60);

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Sunlit Animate project",
            SuggestedFileName = $"{_project.Name}.sanp",
            DefaultExtension = "sanp",
            FileTypeChoices =
            [
                new FilePickerFileType("Sunlit Animate Project") { Patterns = ["*.sanp"] }
            ]
        });

        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await JsonSerializer.SerializeAsync(stream, _project, JsonOptions);
            StatusText.Text = $"Saved {file.Name}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not save project: {ex.Message}";
        }
    }

    private void Brush_Click(object? sender, RoutedEventArgs e)
    {
        _isEraser = false;
        UpdateBrushSettings();
        StatusText.Text = "Brush selected";
    }

    private void Eraser_Click(object? sender, RoutedEventArgs e)
    {
        _isEraser = true;
        UpdateBrushSettings();
        StatusText.Text = "Eraser selected";
    }

    private void Undo_Click(object? sender, RoutedEventArgs e) => CanvasSurface.Undo();
    private void Redo_Click(object? sender, RoutedEventArgs e) => CanvasSurface.Redo();
    private void ClearFrame_Click(object? sender, RoutedEventArgs e) => CanvasSurface.ClearDrawing();

    private void AddFrame_Click(object? sender, RoutedEventArgs e)
    {
        StoreCurrentFrame();
        _project.Frames.Insert(_frameIndex + 1, new AnimationFrame());
        _frameIndex++;
        LoadCurrentFrame();
        StatusText.Text = "Frame added";
    }

    private void DuplicateFrame_Click(object? sender, RoutedEventArgs e)
    {
        StoreCurrentFrame();
        var copy = JsonSerializer.Deserialize<AnimationFrame>(
            JsonSerializer.Serialize(_project.Frames[_frameIndex], JsonOptions), JsonOptions) ?? new AnimationFrame();
        _project.Frames.Insert(_frameIndex + 1, copy);
        _frameIndex++;
        LoadCurrentFrame();
        StatusText.Text = "Frame duplicated";
    }

    private void DeleteFrame_Click(object? sender, RoutedEventArgs e)
    {
        if (_project.Frames.Count <= 1)
        {
            StatusText.Text = "A project must keep at least one frame";
            return;
        }

        _project.Frames.RemoveAt(_frameIndex);
        _frameIndex = Math.Min(_frameIndex, _project.Frames.Count - 1);
        LoadCurrentFrame();
        StatusText.Text = "Frame deleted";
    }

    private void PreviousFrame_Click(object? sender, RoutedEventArgs e)
    {
        StoreCurrentFrame();
        if (_frameIndex > 0)
            _frameIndex--;
        LoadCurrentFrame();
    }

    private void NextFrame_Click(object? sender, RoutedEventArgs e)
    {
        StoreCurrentFrame();
        if (_frameIndex < _project.Frames.Count - 1)
            _frameIndex++;
        LoadCurrentFrame();
    }
}
