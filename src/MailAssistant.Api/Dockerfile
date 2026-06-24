FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/MailAssistant.Domain/MailAssistant.Domain.csproj src/MailAssistant.Domain/
COPY src/MailAssistant.Application/MailAssistant.Application.csproj src/MailAssistant.Application/
COPY src/MailAssistant.Infrastructure/MailAssistant.Infrastructure.csproj src/MailAssistant.Infrastructure/
COPY src/MailAssistant.Api/MailAssistant.Api.csproj src/MailAssistant.Api/
RUN dotnet restore src/MailAssistant.Api/MailAssistant.Api.csproj

COPY src/ src/
RUN dotnet publish src/MailAssistant.Api/MailAssistant.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MailAssistant.Api.dll"]
