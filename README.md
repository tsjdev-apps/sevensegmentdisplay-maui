# Seven Segment Display with .NET MAUI

A .NET MAUI sample app that demonstrates a reusable `SevenSegmentDisplay` control rendered manually with SkiaSharp 4.

![header](/docs/header.png)

## Features

- Reusable MAUI control built on `SKCanvasView`
- Classic seven-segment geometry with rounded polygon segments
- Active, inactive, background, glow, spacing, thickness, radius, opacity, and alignment bindable properties
- Lightweight text-change pulse animation
- Digital clock, countdown, and status display demos
- Light and dark theme support through MAUI `AppThemeBinding`
- MVVM demo state using `CommunityToolkit.Mvvm`
- NUnit coverage for segment mapping and design-space constants
- Central package management through `Directory.Packages.props`

## Screenshots

![screenshot-01](/docs/screenshot-01.png)

![screenshot-02](/docs/screenshot-02.png)

## Repository Structure

```text
.editorconfig
Directory.Build.props
Directory.Packages.props
NuGet.config
MauiSevenSegmentDisplay.slnx
README.md
docs/
└── maui-ui-july-2026-seven-segment-display.md
src/
└── MauiSevenSegmentDisplay/
    ├── Controls/
    ├── Pages/
    ├── ViewModels/
    ├── App.xaml
    ├── AppShell.xaml
    ├── MauiProgram.cs
    └── MauiSevenSegmentDisplay.csproj
tests/
└── MauiSevenSegmentDisplay.Tests/
    ├── SegmentCharacterMapTests.cs
    ├── SevenSegmentOptionsTests.cs
    └── MauiSevenSegmentDisplay.Tests.csproj
```

The generated MAUI `Platforms`, `Properties`, and `Resources` folders are also included so the app can build and run on Android, iOS, Mac Catalyst, and Windows.

## Run

From the repository root:

```bash
dotnet build MauiSevenSegmentDisplay.slnx
dotnet test MauiSevenSegmentDisplay.slnx
```

## SevenSegmentDisplay

`SevenSegmentDisplay` is a reusable control in `src/MauiSevenSegmentDisplay/Controls`. It exposes bindable properties for text, colors, glow, segment sizing, animation, and alignment. The renderer uses a simple design-space model and scales it to the current Skia canvas size, so the display adapts to phones, tablets, and desktop windows.

`SegmentCharacterMap` maps characters to the seven named segment flags. Unsupported characters render as blanks, which keeps the control safe for arbitrary bound text.

## Tests

The NUnit test project lives in `tests/MauiSevenSegmentDisplay.Tests`. It verifies the character-to-segment mapping and the design-space constants that drive the Skia renderer.

## Example XAML

```xml
<controls:SevenSegmentDisplay
    Text="{Binding CurrentTime}"
    SegmentColor="Lime"
    InactiveSegmentColor="DarkGreen"
    DisplayBackgroundColor="Black"
    GlowColor="Lime"
    SegmentThickness="12"
    CharacterSpacing="8"
    SegmentCornerRadius="4"
    InactiveSegmentOpacity="0.28"
    GlowIntensity="0.7"
    IsGlowEnabled="True"
    IsAnimationEnabled="True"
    AnimationDuration="250" />
```
