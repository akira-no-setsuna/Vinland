using System.Diagnostics;
using Serilog;
using Vinland.Core.Infrastructure.Services.Threads;

public class ThreadManager : IDisposable
{
    private readonly BaseThread[] _threads;
    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _runningTasks = new();
    private readonly object _gate = new();

    private bool _started;
    private bool _stopRequested;
    private bool _disposed;

    public ThreadManager(List<BaseThread> threads)
    {
        ArgumentNullException.ThrowIfNull(threads);
        _threads = [.. threads];
    }

    public void Start()
    {
        lock (_gate)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ThreadManager));
            if (_started)  throw new InvalidOperationException("Already started");

            _started = true;

            foreach (var thread in _threads)
            {
                var task = Task.Factory.StartNew(
                    () => ThreadLoop(thread),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                _runningTasks.Add(task);
            }
        }

        Log.Debug("Threads started.");
    }

    public void Stop()
    {
        Task[] snapshot;
        lock (_gate)
        {
            if (!_started || _stopRequested || _disposed) return;

            _stopRequested = true;
            _cts.Cancel();
            snapshot = _runningTasks.ToArray();
        }

        WaitAllSafe(snapshot);
        Log.Debug("Threads stopped.");
    }

    public void Dispose()
    {
        Task[] snapshot;
        CancellationTokenSource cts;

        lock (_gate)
        {
            if (_disposed) return;

            _disposed = true;
            _cts.Cancel();
            snapshot = _runningTasks.ToArray();
            cts = _cts;
        }

        try
        {
            WaitAllSafe(snapshot);
        }
        finally
        {
            cts.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private static void WaitAllSafe(Task[] tasks)
    {
        if (tasks.Length == 0) return;

        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException ex)
        {
            foreach (var inner in ex.Flatten().InnerExceptions)
                Log.Error(inner, "Thread task faulted during shutdown.");
        }
    }

    private void ThreadLoop(BaseThread thread)
    {
        var token = _cts.Token;
        var stopwatch = Stopwatch.StartNew();
        double lastFrameTime = stopwatch.Elapsed.TotalSeconds;

        const double maxFrameTime = 1.0 / 240.0;
        const double maxDelta     = 0.1;

        try
        {
            Thread.CurrentThread.Name         = thread.ThreadName;
            Thread.CurrentThread.Priority     = ThreadPriority.AboveNormal;
            Thread.CurrentThread.IsBackground = true;

            while (!token.IsCancellationRequested)
            {
                double frameStart = stopwatch.Elapsed.TotalSeconds;
                double deltaTime  = frameStart - lastFrameTime;
                lastFrameTime     = frameStart;

                if (deltaTime > maxDelta) deltaTime = maxDelta;
                if (deltaTime > 0.0)      thread.ManualUpdate((float)deltaTime);

                double targetTime = frameStart + maxFrameTime;
                double now        = stopwatch.Elapsed.TotalSeconds;

                if (now < targetTime)
                {
                    double toSleep = targetTime - now;
                    if (toSleep > 0.002)
                        Thread.Sleep(TimeSpan.FromSeconds(toSleep - 0.001));

                    while (stopwatch.Elapsed.TotalSeconds < targetTime && !token.IsCancellationRequested)
                        Thread.SpinWait(10);
                }
            }
        }
        catch (OperationCanceledException) { /* нормальная отмена */ }
        catch (Exception ex)
        {
            Log.Error(ex, "{Name} thread crashed.", thread.ThreadName);
        }
    }
}