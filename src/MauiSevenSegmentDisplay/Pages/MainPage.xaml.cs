using MauiSevenSegmentDisplay.ViewModels;

namespace MauiSevenSegmentDisplay.Pages;

/// <summary>
/// Demo page that binds its UI to <see cref="MainViewModel"/>.
/// </summary>
public partial class MainPage : ContentPage
{
    /// <summary>
    /// Initializes the XAML page and assigns the injected ViewModel as binding context.
    /// </summary>
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
