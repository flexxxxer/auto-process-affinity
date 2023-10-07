$scriptPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$csprojPath = Join-Path $scriptPath "UI.Desktop\UI.Desktop.csproj"

dotnet publish $csprojPath `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true

Write-Output "Done."
Invoke-Item -Path (Join-Path $scriptPath "UI.Desktop\bin\Release\net7.0\win-x64\publish")