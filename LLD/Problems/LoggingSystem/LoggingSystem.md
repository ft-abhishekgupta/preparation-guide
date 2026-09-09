# Logging System
Design a logging system with configurable log levels and output destination.

## Requirement
```
The logging framework should support different log levels, such as DEBUG, INFO, WARNING, ERROR, and FATAL.
It should allow logging messages with a timestamp, log level, and message content.
The framework should support multiple output destinations, such as console, file, and database.
It should provide a configuration mechanism to set the log level and output destination.
The logging framework should be thread-safe to handle concurrent logging from multiple threads.
It should be extensible to accommodate new log levels and output destinations in the future.
```
## Class Design
```
enum LogLevel
    Debug
    Info
    Warning
    Error
    Fatal

class LogMessage
    - timestamp: DateTime
    - level: LogLevel
    - message: string

interface ILogDestination
    + Write(log: LogMessage)

class ConsoleDestination implements ILogDestination
    + Write(log)

class FileDestination implements ILogDestination
    - filePath: string
    - lock: object
    + Write(log)

class DatabaseDestination implements ILogDestination
    + Write(log)

class LogProcessorWorker
    - queue: BlockingCollection<LogMessage>
    - destinations: List<ILogDestination>
    - workerTask: Task

    + Start()
    + Stop()

    - ProcessLogs()
    - ProcessLog(log)

class LoggingSystem
    - queue: BlockingCollection<LogMessage>
    - destinations: List<ILogDestination>
    - workers: List<LogProcessorWorker>
    - minimumLevel: LogLevel

    + LoggingSystem(...)
    + Debug(message)
    + Info(message)
    + Warning(message)
    + Error(message)
    + Fatal(message)
    + Dispose()

    - Log(level, message)
```
```
        LoggingSystem
             |
 owns       / \
           /   \
          ↓     ↓
     Blocking   List<ILogDestination>
     Collection       |
                      |
           ┌──────────┼──────────┐
           ↓          ↓          ↓
       Console      File      Database

PRODUCER-CONSUMER

Application
    |
    | Info("User logged in")
    ↓
LoggingSystem
    |
    | queue.Add(log)
    ↓
BlockingCollection<LogMessage>
    |
    | queue.Take()
    ↓
Worker Thread
    |
    ↓
ILogDestination
```
## Implementation
LogLevel
```cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}
```
LogMessage
```cs
public class LogMessage
{
    public DateTime Timestamp { get; }
    public LogLevel Level { get; }
    public string Message { get; }

    public LogMessage(LogLevel level, string message)
    {
        Timestamp = DateTime.UtcNow;
        Level = level;
        Message = message;
    }
}
```
Destination
```cs
public interface ILogDestination
{
    void Write(LogMessage log);
}
```
ConsoleDestination
```cs
public class ConsoleDestination : ILogDestination
{
    public void Write(LogMessage log)
    {
        Console.WriteLine(
            $"[{log.Timestamp:O}] " +
            $"[{log.Level}] " +
            $"{log.Message}");
    }
}
```
FileDestination
```cs
public class FileDestination : ILogDestination
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileDestination(string filePath)
    {
        _filePath = filePath;
    }

    public void Write(LogMessage log)
    {
        lock (_lock)
        {
            File.AppendAllText(
                _filePath,
                $"[{log.Timestamp:O}] " +
                $"[{log.Level}] " +
                $"{log.Message}" +
                Environment.NewLine);
        }
    }
}
```
DatabaseDestination
```cs
public class DatabaseDestination : ILogDestination
{
    public void Write(LogMessage log)
    {
        // Insert log into database
        Console.WriteLine(
            $"DB: [{log.Level}] {log.Message}");
    }
}
```
LoggingProcessorWorker
```cs
public class LogProcessorWorker
{
    private readonly BlockingCollection<LogMessage> _queue;
    private readonly IReadOnlyList<ILogDestination> _destinations;

    private Task? _workerTask;

    public LogProcessorWorker(
        BlockingCollection<LogMessage> queue,
        IEnumerable<ILogDestination> destinations)
    {
        _queue = queue;
        _destinations = destinations.ToList();
    }

    public void Start()
    {
        _workerTask = Task.Run(ProcessLogs);
    }

    private void ProcessLogs()
    {
        while (true)
        {
            try
            {
                // Blocks until a log is available.
                var log = _queue.Take();

                ProcessLog(log);
            }
            catch (InvalidOperationException)
            {
                // Queue completed and empty.
                break;
            }
        }
    }

    private void ProcessLog(LogMessage log)
    {
        foreach (var destination in _destinations)
        {
            try
            {
                destination.Write(log);
            }
            catch (Exception ex)
            {
                // One destination should not
                // kill the worker.
                Console.Error.WriteLine(
                    $"Destination failed: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _workerTask?.Wait();
    }
}
```
LoggingSystem
```cs
public class LoggingSystem : IDisposable
{
    private readonly BlockingCollection<LogMessage> _queue;

    private readonly List<ILogDestination> _destinations;
    private readonly List<LogProcessorWorker> _workers;

    private readonly LogLevel _minimumLevel;

    public LoggingSystem(
        LogLevel minimumLevel,
        IEnumerable<ILogDestination> destinations,
        int workerCount = 4)
    {
        _minimumLevel = minimumLevel;

        _destinations = destinations.ToList();

        _queue = new BlockingCollection<LogMessage>(
            new ConcurrentQueue<LogMessage>());

        _workers = new List<LogProcessorWorker>();

        StartWorkers(workerCount);
    }

    private void StartWorkers(int workerCount)
    {
        for (int i = 0; i < workerCount; i++)
        {
            var worker = new LogProcessorWorker(
                _queue,
                _destinations);

            worker.Start();
            _workers.Add(worker);
        }
    }

    public void Debug(string message)
        => Log(LogLevel.Debug, message);

    public void Info(string message)
        => Log(LogLevel.Info, message);

    public void Warning(string message)
        => Log(LogLevel.Warning, message);

    public void Error(string message)
        => Log(LogLevel.Error, message);

    public void Fatal(string message)
        => Log(LogLevel.Fatal, message);

    private void Log(
        LogLevel level,
        string message)
    {
        if (level < _minimumLevel)
        {
            return;
        }

        var log = new LogMessage(
            level,
            message);

        _queue.Add(log);
    }

    public void Dispose()
    {
        // No more messages can be added.
        _queue.CompleteAdding();

        // Workers finish remaining logs.
        foreach (var worker in _workers)
        {
            worker.Stop();
        }

        _queue.Dispose();
    }
}
```
## Usage
```cs
using var logger = new LoggingSystem(
    LogLevel.Debug,
    new ILogDestination[]
    {
        new ConsoleDestination(),
        new FileDestination("application.log"),
        new DatabaseDestination()
    },
    workerCount: 4);

logger.Debug("Application started");
logger.Info("User logged in");
logger.Warning("Memory usage is high");
logger.Error("Payment failed");
logger.Fatal("Database unavailable");
```