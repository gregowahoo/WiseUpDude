# Run this script from the root of your project (where your .sln is)
# It finds duplicate static asset file names (case-insensitive) under common web asset folders

$staticFolders = @("wwwroot", "static", "public", "assets")
$allFiles = @()

foreach ($folder in $staticFolders) {
    if (Test-Path $folder) {
        $allFiles += Get-ChildItem -Path $folder -Recurse -File | Select-Object -Property FullName, Name
    }
}

$duplicates = $allFiles | Group-Object { $_.Name.ToLower() } | Where-Object { $_.Count -gt 1 }

if ($duplicates.Count -eq 0) {
    Write-Host "No duplicate static asset file names found."
} else {
    Write-Host "Duplicate static asset file names detected:`n"
    foreach ($group in $duplicates) {
        Write-Host "Filename: $($group.Name)"
        foreach ($file in $group.Group) {
            Write-Host "  $($file.FullName)"
        }
        Write-Host ""
    }
}