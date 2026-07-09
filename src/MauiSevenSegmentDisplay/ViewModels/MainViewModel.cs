using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiSevenSegmentDisplay.ViewModels;

/// <summary>
/// ViewModel for the sample page; it owns all demo state so the page code-behind stays minimal.
/// </summary>
public sealed class MainViewModel : ObservableObject, IDisposable
{
    // Short status words chosen to fit a seven-segment display reasonably well.
    private static readonly string[] Statuses = ["READY", "LOAD", "ERROR", "DONE", "PAUSE"];

    // PeriodicTimer avoids platform-specific timer lifetimes and is cancelled during disposal.
    private readonly CancellationTokenSource _timerCancellation = new();
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));

    // The reset command and timer tick can both touch the countdown, so keep updates atomic.
    private readonly object _countdownLock = new();

    private int _statusIndex;
    private TimeSpan _remaining = TimeSpan.FromMinutes(10);
    private string _currentTime = DateTime.Now.ToString("HH:mm:ss");
    private string _countdownText = "10:00";
    private string _statusText = Statuses[0];
    private bool _disposed;

    /// <summary>
    /// Creates commands and starts the lightweight timer loop used by the demo.
    /// </summary>
    public MainViewModel()
    {
        ResetCountdownCommand = new RelayCommand(ResetCountdown);
        NextStatusCommand = new RelayCommand(NextStatus);

        // The demo data lives in the ViewModel: the page only binds to these
        // properties, keeping timer/countdown/status logic out of code-behind.
        _ = RunClockAsync(_timerCancellation.Token);
    }

    /// <summary>
    /// Gets the current local time formatted for the seven-segment clock display.
    /// </summary>
    public string CurrentTime
    {
        get => _currentTime;
        private set => SetProperty(ref _currentTime, value);
    }

    /// <summary>
    /// Gets the remaining countdown time formatted as MM:ss.
    /// </summary>
    public string CountdownText
    {
        get => _countdownText;
        private set => SetProperty(ref _countdownText, value);
    }

    /// <summary>
    /// Gets the current status word shown in the status display.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    /// <summary>
    /// Resets the countdown display back to ten minutes.
    /// </summary>
    public IRelayCommand ResetCountdownCommand { get; }

    /// <summary>
    /// Advances the status display to the next sample word.
    /// </summary>
    public IRelayCommand NextStatusCommand { get; }

    /// <summary>
    /// Stops the timer loop and releases timer resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timerCancellation.Cancel();
        _timerCancellation.Dispose();
        _timer.Dispose();
    }

    // Runs the one-second tick loop off the UI thread, then marshals property updates to the main thread.
    private async Task RunClockAsync(CancellationToken cancellationToken)
    {
        UpdateClockValues();

        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                MainThread.BeginInvokeOnMainThread(UpdateClockValues);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the singleton ViewModel is disposed during app shutdown.
        }
    }

    // Updates both the clock text and countdown text in one place so the timer loop stays simple.
    private void UpdateClockValues()
    {
        CurrentTime = DateTime.Now.ToString("HH:mm:ss");

        lock (_countdownLock)
        {
            if (_remaining > TimeSpan.Zero)
            {
                _remaining -= TimeSpan.FromSeconds(1);
            }

            CountdownText = $"{(int)_remaining.TotalMinutes:00}:{_remaining.Seconds:00}";
        }
    }

    // Command target for returning the countdown to its starting value.
    private void ResetCountdown()
    {
        lock (_countdownLock)
        {
            _remaining = TimeSpan.FromMinutes(10);
            CountdownText = "10:00";
        }
    }

    // Command target for cycling through the small set of display-friendly status words.
    private void NextStatus()
    {
        _statusIndex = (_statusIndex + 1) % Statuses.Length;
        StatusText = Statuses[_statusIndex];
    }
}
