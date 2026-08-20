#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Game.Core.Infrastructure.Services;

public class GameThreadManager : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Task? _dataThread;

    private bool _disposed;

    private Task? _logicThread;
    private Task? _physicsThread;
    private int _started;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            throw new InvalidOperationException("Already started");

        _logicThread = Task.Factory.StartNew(
            LogicLoop,
            _cts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
        _physicsThread = Task.Factory.StartNew(
            PhysicsLoop,
            _cts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();
        _dataThread = Task.Run(DataLoop);

        Log.Debug("Threads started.");
    }

    private async Task LogicLoop() // For now, it's just a placeholder
    {
        try
        {
            Thread.CurrentThread.Name = "LogicThread";
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            Thread.CurrentThread.IsBackground = true;

            var token = _cts.Token;
            while (!token.IsCancellationRequested) await Task.Delay(16, token);
        }
        catch (OperationCanceledException)
        {
            // If thread Cancel()
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Thread crashed");
        }
    }

    private async Task PhysicsLoop() // For now, it's just a placeholder
    {
        try
        {
            Thread.CurrentThread.Name = "PhysicsThread";
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
            Thread.CurrentThread.IsBackground = true;

            var token = _cts.Token;
            while (!token.IsCancellationRequested) await Task.Delay(16, token);
        }
        catch (OperationCanceledException)
        {
            // If thread Cancel()
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Thread crashed");
        }
    }

    private async Task DataLoop() // For now, it's just a placeholder
    {
        try
        {
            Thread.CurrentThread.Name = "DataThread";
            Thread.CurrentThread.IsBackground = true;

            var token = _cts.Token;
            while (!token.IsCancellationRequested) await Task.Delay(16, token);
        }
        catch (OperationCanceledException)
        {
            // If thread Cancel()
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Thread crashed");
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        Log.Debug("Stop signal sent to threads.");
    }

    public bool Join(TimeSpan timeout)
    {
        if (_logicThread == null) return true;

        var tasks = new[] { _logicThread, _physicsThread, _dataThread }
            .Where(t => t != null)
            .Select(t => t!)
            .ToArray();

        if (tasks.Length == 0) return true;

        var stopwatch = Stopwatch.StartNew();
        const int sleepMs = 5;

        while (stopwatch.Elapsed < timeout)
        {
            if (tasks.All(t => t.IsCompleted))
            {
                foreach (var task in tasks)
                    if (task.IsFaulted)
                    {
                        Log.Error(task.Exception, "Background thread faulted unexpectedly.");
                        return false;
                    }

                return true;
            }

            Thread.Sleep(sleepMs);
        }

        return false;
    }

    public void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Stop();

            if (!Join(TimeSpan.FromMilliseconds(100)))
                Log.Warning(
                    "Thread shutdown timeout. Logic alive: {LogicAlive}, Physics alive: {PhysicsAlive}, Data alive: {DataAlive}",
                    _logicThread is { IsCompleted: false },
                    _physicsThread is { IsCompleted: false },
                    _dataThread is { IsCompleted: false });
            Log.Debug("Threads disposed.");
            _cts.Dispose();
        }

        _disposed = true;
    }
}