namespace MailAssistant.Worker;

public sealed partial class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "MailAssistant worker started.")]
    private static partial void LogStarted(ILogger logger);
}
