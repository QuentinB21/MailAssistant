$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$apiBaseUrl = "http://localhost:5080"
$tokenUrl = "http://localhost:8080/realms/mailassistant/protocol/openid-connect/token"
$stdout = Join-Path $env:TEMP "mailassistant-auth-test.stdout.log"
$stderr = Join-Path $env:TEMP "mailassistant-auth-test.stderr.log"

function Get-AccessToken {
    param(
        [Parameter(Mandatory)]
        [string]$Username,
        [Parameter(Mandatory)]
        [string]$Password
    )

    $response = Invoke-RestMethod `
        -Method Post `
        -Uri $tokenUrl `
        -ContentType "application/x-www-form-urlencoded" `
        -Body @{
            client_id  = "mailassistant-tests"
            grant_type = "password"
            username   = $Username
            password   = $Password
            scope      = "openid profile email"
        }

    return $response.access_token
}

function Invoke-AuthenticatedApi {
    param(
        [Parameter(Mandatory)]
        [string]$Method,
        [Parameter(Mandatory)]
        [string]$Uri,
        [Parameter(Mandatory)]
        [string]$Token,
        [object]$Body
    )

    $parameters = @{
        Method  = $Method
        Uri     = $Uri
        Headers = @{ Authorization = "Bearer $Token" }
    }

    if ($null -ne $Body) {
        $parameters.ContentType = "application/json"
        $parameters.Body = $Body | ConvertTo-Json
    }

    return Invoke-RestMethod @parameters
}

function Get-HttpStatus {
    param([Parameter(Mandatory)][scriptblock]$Action)

    try {
        & $Action | Out-Null
        return 200
    }
    catch {
        return [int]$_.Exception.Response.StatusCode
    }
}

Push-Location $root
try {
    docker compose up -d postgres keycloak | Out-Host
    dotnet ef database update `
        --project src/MailAssistant.Infrastructure `
        --startup-project src/MailAssistant.Api | Out-Host
    dotnet build MailAssistant.sln --configuration Release | Out-Host

    Remove-Item -LiteralPath $stdout, $stderr -Force -ErrorAction SilentlyContinue
    $api = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @(
            "run",
            "--project",
            "src/MailAssistant.Api",
            "--no-build",
            "--configuration",
            "Release"
        ) `
        -WorkingDirectory $root `
        -WindowStyle Hidden `
        -RedirectStandardOutput $stdout `
        -RedirectStandardError $stderr `
        -PassThru

    try {
        $ready = $false
        for ($attempt = 0; $attempt -lt 30; $attempt++) {
            try {
                $health = Invoke-WebRequest `
                    -Uri "$apiBaseUrl/health" `
                    -UseBasicParsing `
                    -TimeoutSec 2
                if ($health.StatusCode -eq 200) {
                    $ready = $true
                    break
                }
            }
            catch {
                Start-Sleep -Milliseconds 500
            }
        }

        if (-not $ready) {
            throw "MailAssistant API did not become ready."
        }

        $owner = Get-AccessToken "owner" "Owner-local-2026!"
        $admin = Get-AccessToken "admin" "Admin-local-2026!"
        $member = Get-AccessToken "member" "Member-local-2026!"
        $outsider = Get-AccessToken "outsider" "Outsider-local-2026!"

        foreach ($token in @($owner, $admin, $member, $outsider)) {
            Invoke-AuthenticatedApi GET "$apiBaseUrl/api/me" $token | Out-Null
        }

        $unauthenticated = Get-HttpStatus {
            Invoke-RestMethod "$apiBaseUrl/api/organizations"
        }
        $suffix = [Guid]::NewGuid().ToString("N").Substring(0, 8)
        $organization = Invoke-AuthenticatedApi `
            POST `
            "$apiBaseUrl/api/organizations" `
            $owner `
            @{ name = "Authorization test $suffix" }
        $organizationUrl = "$apiBaseUrl/api/organizations/$($organization.id)"

        Invoke-AuthenticatedApi `
            PUT `
            "$organizationUrl/members/by-email" `
            $owner `
            @{ email = "admin@local.test"; role = "Admin" } | Out-Null
        Invoke-AuthenticatedApi `
            PUT `
            "$organizationUrl/members/by-email" `
            $owner `
            @{ email = "member@local.test"; role = "Member" } | Out-Null

        Invoke-AuthenticatedApi `
            POST `
            "$organizationUrl/projects" `
            $admin `
            @{
                name                     = "Admin project"
                classificationTargetName = "Admin project"
                description              = $null
            } | Out-Null

        $memberRead = Get-HttpStatus {
            Invoke-AuthenticatedApi GET "$organizationUrl/projects" $member
        }
        $memberWrite = Get-HttpStatus {
            Invoke-AuthenticatedApi `
                POST `
                "$organizationUrl/projects" `
                $member `
                @{
                    name                     = "Forbidden"
                    classificationTargetName = "Forbidden"
                    description              = $null
                }
        }
        $outsiderRead = Get-HttpStatus {
            Invoke-AuthenticatedApi GET "$organizationUrl/projects" $outsider
        }

        if ($unauthenticated -ne 401 `
            -or $memberRead -ne 200 `
            -or $memberWrite -ne 403 `
            -or $outsiderRead -ne 403) {
            throw "Authorization checks failed."
        }

        Write-Host "Authentication and tenant authorization checks passed."
    }
    finally {
        if (-not $api.HasExited) {
            Stop-Process -Id $api.Id -Force
        }
    }
}
finally {
    Pop-Location
}
