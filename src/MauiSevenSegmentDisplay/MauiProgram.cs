using MauiSevenSegmentDisplay.Pages;
using MauiSevenSegmentDisplay.ViewModels;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiSevenSegmentDisplay;

/// <summary>
/// Central MAUI startup class that configures fonts, SkiaSharp, logging, and app services.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds the MAUI app with the latest installed MAUI workload and centrally managed NuGet packages.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
