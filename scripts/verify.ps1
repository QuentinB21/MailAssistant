$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

Push-Location $root
try {
    $nodeVersionText = (node --version).TrimStart("v").Split("-")[0]
    $nodeVersion = [Version]$nodeVersionText
    $minimumNodeVersion = [Version]"22.12.0"

    if ($nodeVersion -lt $minimumNodeVersion) {
        throw "Node.js $minimumNodeVersion ou plus récent est requis. Version détectée : $nodeVersion."
    }

    dotnet restore MailAssistant.sln
    dotnet build MailAssistant.sln --no-restore --configuration Release
    dotnet test MailAssistant.sln --no-build --configuration Release

    Push-Location "frontend"
    try {
        npm ci
        npm run lint
        npm run test
        npm run build
    }
    finally {
        Pop-Location
    }

    docker compose config --quiet
}
finally {
    Pop-Location
}
