Sunlit Animate desktop starter

This is a C# desktop application developed with Avalonia UI. This is not a browser app.

Requirements:
- .NET 10 SDK
- macOS/Windows
- Internet access during the first build so that NuGet could install Avalonia packages

Run from the repository root:
    dotnet restore src/SunlitAnimate/SunlitAnimate.csproj
    dotnet run --project src/SunlitAnimate/SunlitAnimate.csproj

Features of the current starter:
- Desktop application window
- Drawing with a brush and eraser on the canvas
- Brush color and size
- Undo/Redo of strokes
- Addition, duplication, deletion, navigation of frames
- Saving/opening .sanp projects
- FPS value saved in the project

This is the base for development, but not the animation editor itself. Playback, layers, onion skinning, export, autosave/recovery, automated tests need to be implemented. The source has not been compiled/tested in this environment yet.

In order to add it to GitHub, unpack the ZIP archive and upload it contents to the repository root. Maintain the src/SunlitAnimate directory tree structure.
