using MailAssistant.Application;
using MailAssistant.Infrastructure;
using MailAssistant.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddHostedService<Worker>();

var host = builder.Build();
host.Run();

