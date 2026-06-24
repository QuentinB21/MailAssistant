namespace MailAssistant.Application.Common;

public sealed class AuthenticationRequiredException()
    : Exception("Authentication is required.");

public sealed class AccessDeniedException(string message) : Exception(message);
