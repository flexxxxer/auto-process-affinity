#!/bin/bash

scriptPath=$(dirname "$(readlink -f "$0")")
csprojPath="$scriptPath/UI.Desktop/UI.Desktop.csproj"

dotnet publish "$csprojPath" \
  --configuration Release \
  --runtime linux-x64 \
  --self-contained true \
  --output "./.builds/linux-x64" \
  /p:PublishSingleFile=true \
  /p:DebugType=None \
  /p:DebugSymbols=false

echo "Done."