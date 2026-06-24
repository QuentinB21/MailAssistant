using MailAssistant.Application;
using MailAssistant.Application.Abstractions;
using MailAssistant.Infrastructure;
using MailAssistant.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddSingleton<ICurrentUser, WorkerCurrentUser>()
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();
