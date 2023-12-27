$scriptPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$csprojPath = Join-Path $scriptPath "UI.Desktop\UI.Desktop.csproj"

dotnet publish $csprojPath `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output .\.builds\win-x64 `
  /p:PublishSingleFile=true `
  /p:DebugType=None `
  /p:DebugSymbols=false

Write-Output "Done."