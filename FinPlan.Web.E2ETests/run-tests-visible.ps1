# Run Playwright tests with visible browser
$env:HEADED = "1"
$env:BROWSER = "msedge"

# Run all tests
dotnet test --logger:"console;verbosity=detailed"
