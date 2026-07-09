namespace MauiSevenSegmentDisplay;

/// <summary>
/// Application root that receives the Shell from dependency injection.
/// </summary>
public partial class App : Application
{
    // Keep the injected shell so each app window uses the same configured navigation root.
    private readonly AppShell _appShell;

    /// <summary>
    /// Initializes application resources and stores the injected shell.
    /// </summary>
    public App(AppShell appShell)
    {
        InitializeComponent();
        _appShell = appShell;
    }

    /// <summary>
    /// Creates the first application window using the DI-configured shell.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_appShell);
    }
}
