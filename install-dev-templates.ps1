# Inspired by https://github.com/AvaloniaUI/avalonia-dotnet-templates/blob/main/install-dev-templates.ps1

######################################################################
# Step 2: Uninstall previous templates and clean output
######################################################################
dotnet new uninstall Akka.Templates
Remove-Item bin/**/*.nupkg

######################################################################
# Step 3: Pack new templates
######################################################################
dotnet pack -c Release
# Search Directory
$directoryPath = ".\bin\Release"

$latestNupkgFile = Get-ChildItem -Path $directoryPath -Recurse -Filter "*.nupkg" |
                   Where-Object { -not $_.PSIsContainer } |
                   Sort-Object LastWriteTime -Descending |
                   Select-Object -First 1

######################################################################
# Step 4: install the templates
######################################################################
if ($latestNupkgFile) {
  $latestNupkgPath = $latestNupkgFile.FullName
  dotnet new install $latestNupkgPath
}