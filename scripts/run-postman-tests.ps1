param(
    [Parameter(Mandatory = $true)]
    [string]$AdminPassword,

    [Parameter(Mandatory = $true)]
    [string]$UserPassword,

    [string]$AdminEmail = "admin@quiz.local",
    [string]$UserEmail = "user@quiz.local",
    [string]$BaseUrl = "http://localhost:5169",
    [string]$CollectionFile = "tests/postman/DAF-Quiz.postman_collection.json",
    [string]$EnvironmentFile = "tests/postman/DAF-Quiz.postman_environment.json",
    [string]$JunitOutput = "tests/postman/results.xml"
)

$ErrorActionPreference = "Stop"

function Ensure-Command {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found. Install Node.js/npm first."
    }
}

Ensure-Command -Name "npm"
Ensure-Command -Name "npx"

if (-not (Test-Path -LiteralPath $CollectionFile)) {
    throw "Collection file not found: $CollectionFile"
}

if (-not (Test-Path -LiteralPath $EnvironmentFile)) {
    throw "Environment file not found: $EnvironmentFile"
}

$junitDir = Split-Path -Parent $JunitOutput
if ($junitDir -and -not (Test-Path -LiteralPath $junitDir)) {
    New-Item -ItemType Directory -Path $junitDir | Out-Null
}

npm install --no-audit --no-fund --save-dev newman newman-reporter-junit
if ($LASTEXITCODE -ne 0) {
    throw "Failed to install Newman dependencies."
}

npx newman run $CollectionFile `
    --environment $EnvironmentFile `
    --env-var "baseUrl=$BaseUrl" `
    --env-var "adminEmail=$AdminEmail" `
    --env-var "adminPassword=$AdminPassword" `
    --env-var "userEmail=$UserEmail" `
    --env-var "userPassword=$UserPassword" `
    --reporters "cli,junit" `
    --reporter-junit-export $JunitOutput

if ($LASTEXITCODE -ne 0) {
    throw "Newman tests failed."
}

Write-Host "Postman/Newman tests completed successfully."
Write-Host "JUnit report: $JunitOutput"
