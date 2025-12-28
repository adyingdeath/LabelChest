namespace LabelChest.Utils;

public class Debouncer
{
    private readonly TimeSpan _delay;
    private readonly Action _action;
    private Timer? _timer;
    private readonly object _lockObject = new object();

    public Debouncer(TimeSpan delay, Action action)
    {
        _delay = delay;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Invoke()
    {
        lock (_lockObject)
        {
            _timer?.Dispose();
            _timer = new Timer(_ => 
            {
                _action();
                _timer?.Dispose();
                _timer = null;
            }, null, _delay, Timeout.InfiniteTimeSpan);
        }
    }

    public void Cancel()
    {
        lock (_lockObject)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    public void Dispose()
    {
        Cancel();
    }
}