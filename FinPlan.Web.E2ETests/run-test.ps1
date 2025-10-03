# Run specific Playwright test with visible browser
# Usage: .\run-test.ps1 "TestName"
# Example: .\run-test.ps1 "SavingsPage_ShouldLoadSuccessfully"

param(
    [Parameter(Mandatory=$true)]
    [string]$TestName
)

$env:HEADED = "1"
$env:BROWSER = "msedge"

Write-Host "Running test: $TestName" -ForegroundColor Green
dotnet test --filter $TestName --logger:"console;verbosity=detailed"
