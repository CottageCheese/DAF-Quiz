param(
    [switch]$Headless
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Ensure-Command {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' not found."
    }
}

Ensure-Command -Name "dotnet"
Ensure-Command -Name "npm"

# ── .NET ────────────────────────────────────────────────────────────────────

Step ".NET restore"
dotnet restore "$root/QuizProject.sln"
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed." }

Step ".NET build"
dotnet build "$root/QuizProject.sln" --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

Step ".NET tests"
dotnet test "$root/QuizProject.sln" --no-build --configuration Release
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed." }

# ── Angular ──────────────────────────────────────────────────────────────────

Step "Angular install"
Push-Location "$root/quiz-angular"
try {
    npm ci
    if ($LASTEXITCODE -ne 0) { throw "npm ci failed." }

    Step "Angular build"
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "Angular build failed." }

    Step "Angular tests"
    $browser = if ($Headless -or $env:CI) { "ChromeHeadless" } else { "Chrome" }
    npm test -- --watch=false --browsers=$browser
    if ($LASTEXITCODE -ne 0) { throw "Angular tests failed." }
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "All builds and tests passed." -ForegroundColor Green
