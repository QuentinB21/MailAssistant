FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/MailAssistant.Domain/MailAssistant.Domain.csproj src/MailAssistant.Domain/
COPY src/MailAssistant.Application/MailAssistant.Application.csproj src/MailAssistant.Application/
COPY src/MailAssistant.Infrastructure/MailAssistant.Infrastructure.csproj src/MailAssistant.Infrastructure/
COPY src/MailAssistant.Worker/MailAssistant.Worker.csproj src/MailAssistant.Worker/
RUN dotnet restore src/MailAssistant.Worker/MailAssistant.Worker.csproj

COPY src/ src/
RUN dotnet publish src/MailAssistant.Worker/MailAssistant.Worker.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MailAssistant.Worker.dll"]
