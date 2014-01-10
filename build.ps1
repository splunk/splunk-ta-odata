param(
    # Don't build (only do this if you just need to repackage the Output folder)
    [switch]$SkipBuild,
    [switch]$NoPackage
)

if(!$PSScriptRoot){[string]$PSScriptRoot = $PWD}

if(!$SkipBuild) {
    # Make sure the .net folder is in your PATH. Silly Windows.
    if(!($Path -split ";" | Where { $_ -eq [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()})) {
      $Env:Path += ";" + [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
    }

    # msbuild $PSScriptRoot\MakePackage\MakePackage.csproj /t:CopyBits /p:platform=x86 /p:Configuration=Release /p:SolutionDir=$PSScriptRoot\
    msbuild $PSScriptRoot\MakePackage\MakePackage.csproj /t:CopyBits /p:platform=x64 /p:Configuration=Release /p:SolutionDir=$PSScriptRoot\
}

# For now, need to manually remove the .Net 4.5 reference from the 3.5 config file
# So we're building both x86 and x64 with /t:CopyBits (instead of /t:Package)
# And then we'll edit the files and create the .tar.gz package "by hand" here:

$AssemblyName = "SA-ModularInput-OData"
$Env:OutputPath = Join-Path $PSScriptRoot "Output\Release"

$OutputPath = Join-Path $Env:OutputPath "${AssemblyName}"

# Remove the old package
Remove-Item "${OutputPath}.tar.gz" -Force -ErrorAction SilentlyContinue

# Remove any visual studio junk
Get-ChildItem $OutputPath -recurse -filter *vshost.exe* | Remove-Item

if(!$NoPackage) {

    if(!(Get-ChildItem $OutputPath -ErrorAction SilentlyContinue)) { 
        Write-Warning "No output to package. Skipping Package step"
    } else {
        # Sadly, we have to shell out to cmd in order to do .tar.gz files in one step with 7z.exe
        $PrePath = $Env:Path
        $Env:PATH = $Env:Path += ";$(Join-Path $PSScriptRoot .nuget)"
        # PowerShell can't handle binary pipes, and we want to avoid the intermediate .tar file:
        cmd /V:ON /c "7za.exe a -ttar -so -x!*.vshost.exe -x!*.7z -x!*.pdb -x!*.gz -x!*.zip stdout ""!OutputPath!\*"" | 7za.exe a -si -tgzip -mx9 -bd ""!OutputPath!\${AssemblyName}.tar.gz"""
        $Env:Path = $PrePath
    }
}