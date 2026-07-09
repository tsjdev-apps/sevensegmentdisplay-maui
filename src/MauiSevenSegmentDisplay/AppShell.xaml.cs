using MauiSevenSegmentDisplay.Pages;

namespace MauiSevenSegmentDisplay;

/// <summary>
/// Shell container that hosts the demo page created by dependency injection.
/// </summary>
public partial class AppShell : Shell
{
    /// <summary>
    /// Adds the already-constructed demo page as the only shell content item.
    /// </summary>
    public AppShell(MainPage mainPage)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Demo",
            Route = nameof(MainPage),
            Content = mainPage
        });
    }
}
