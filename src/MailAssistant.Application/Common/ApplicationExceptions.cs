namespace MailAssistant.Application.Common;

public sealed class AuthenticationRequiredException()
    : Exception("Authentication is required.");

public sealed class AccessDeniedException(string message) : Exception(message);

public sealed class ExternalIntegrationException(string message) : Exception(message);

public sealed class IntegrationNotConfiguredException(string message) : Exception(message);
