<#
PowerShell script to remove comment lines and block comments from source files.
WARNING: This modifies files in-place. Commit or backup before running.
This script finds the project root (first .csproj) and processes files, skipping bin/obj/.git/node_modules.
It removes C# block comments (/* */), Razor block comments (@* *@), HTML comments (<!-- -->), and whole-line // comments.
It avoids removing inline comments after code to reduce risk.
#>

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Allow explicit root override
param(
    [string]$RootPath
)

# Try multiple candidate roots to locate the source files reliably
$parent = Split-Path -Parent $scriptDir
$candidates = @(
    $parent,
    (Join-Path $parent 'Homework-portal'),
    (Join-Path $parent 'Homework-portal\Homework-portal')
)

$found = $null
foreach ($c in $candidates) {
    if (-not (Test-Path $c)) { continue }
    $tryFiles = Get-ChildItem -Path $c -Recurse -File -Include *.cs,*.cshtml,*.razor,*.js,*.css -ErrorAction SilentlyContinue
    if ($tryFiles -and $tryFiles.Count -gt 0) { $found = $c; break }
}

if ($RootPath) { $root = $RootPath } elseif ($found) { $root = $found } else {
    # fallback: try to find .csproj
    $proj = Get-ChildItem -Path $scriptDir -Recurse -Filter *.csproj -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($proj) { $root = Split-Path -Parent $proj.FullName } else { $root = $parent }
}

# File extensions to process
$includeExtensions = '*.cs','*.cshtml','*.razor','*.js','*.css'

$excludePatterns = '\\bin\\','\\obj\\','\\.git\\','\\node_modules\\'

$exts = $includeExtensions -replace '\*',''
$files = Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    $full = $_.FullName
    $extMatch = $exts -contains $_.Extension
    $notExcluded = -not ($excludePatterns | ForEach-Object { $full -match $_ })
    $extMatch -and $notExcluded
}

if (-not $files -or $files.Count -eq 0) {
    Write-Output "No files found to process under $root."
    exit 0
}

foreach ($f in $files) {
    try {
        $text = Get-Content -Raw -LiteralPath $f.FullName -ErrorAction Stop

        # Remove C-style block comments /* ... */
        $text = [regex]::Replace($text, '(?s)/\*.*?\*/', '')
        # Remove Razor block comments @* ... *@
        $text = [regex]::Replace($text, '(?s)@\*.*?\*@', '')
        # Remove HTML comments <!-- ... -->
        $text = [regex]::Replace($text, '(?s)<!--.*?-->', '')
        # Remove single-line comments that occupy the whole line (// ... or /// XML docs)
        $text = [regex]::Replace($text, '(?m)^[ \t]*///?.*$', '')

        # For JS/CSS files, also remove single-line comments
        if ($f.Extension -in '.js','.css') {
            $text = [regex]::Replace($text, '(?m)^[ \t]*//.*$', '')
        }

        # Remove excessive blank lines
        $lines = $text -split "\r?\n"
        $newLines = @()
        $prevEmpty = $false
        foreach ($ln in $lines) {
            if ($ln -match '^[ \t]*$') {
                if (-not $prevEmpty) { $newLines += '' }
                $prevEmpty = $true
            } else {
                $newLines += $ln
                $prevEmpty = $false
            }
        }
        $newText = ($newLines -join "`r`n")

        if ($newText -ne $text) {
            Set-Content -LiteralPath $f.FullName -Value $newText -Encoding UTF8
            Write-Output "Updated: $($f.FullName)"
        }
    } catch {
        Write-Output "Failed: $($f.FullName) - $($_.Exception.Message)"
    }
}

Write-Output "Done."
