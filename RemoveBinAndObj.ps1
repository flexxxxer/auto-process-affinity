$scriptPath = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
$dirsToRemove = Get-ChildItem -Path $scriptPath -Recurse -Directory -Include bin, obj
$dirsToRemove | ForEach-Object {
  Remove-Item -Path $_.FullName -Force -Recurse
}
Write-Output "Done."