using MailAssistant.Application.Abstractions;

namespace MailAssistant.Worker;

internal sealed class WorkerCurrentUser : ICurrentUser
{
    public bool IsAuthenticated => false;

    public string Subject =>
        throw new InvalidOperationException("The worker has no interactive user.");

    public string? Email => null;

    public string DisplayName => "MailAssistant Worker";
}
