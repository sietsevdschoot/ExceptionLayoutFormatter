$token = "oy2kgezuhcxlfy6eouoqgrmfm52dog3e7dh4ebf4wh7444"

$url = "https://api.nuget.org/v3/registration3/automapper/index.json"





function Get-PackageInfoFromFeed {
    param()

    $packageFeed = "https://api-v2v3search-0.nuget.org/query?q=packageid:newtonsoft.Json"

    $response = Invoke-RestMethod -Uri $packageFeed -Verbose:$false

    if ($response.data -eq $null) {
    
        Throw "No packages found.."
    }

    [PsCustomObject]@{ `
        PackageId = $response.data.id; `
        Version = $response.data.version; `
        Description = ($response.data.description.Split('|') | Select -last 1).Trim(); `
    }
}


Function Get-ProjectInfo {
    param(
        [IO.FileInfo] $projectFile
    ) 

    $nuspecData = ([xml](cat $projectFile)).Project.PropertyGroup

    $packageId = if ($nuspecData.PackageId) { $nuspecData.PackageId } else { $nuspecData.AssemblyName }

    $description = $nuspecData.Description

    $projectDll = (dir "$($projectFile.Directory.FullName)\bin\*.dll" -recurse | Sort -prop CreationTime -desc )[0]

    $projectHash = (Get-FileHash -Algorithm MD5 -Path $projectDll).Hash

    [PsCustomObject]@{
        PackageId = $packageId;
        Description = $description;
        Hash = $projectHash;
    }    
}

$solutionRoot = "$PSScriptRoot\..\..\"  

$packageInfo = Get-PackageInfoFromFeed
$projectInfo = Get-ProjectInfo "$solutionRoot\src\ExceptionLayoutFormatter\ExceptionLayoutFormatter.csproj"

$packageInfo | ft
$projectInfo | ft


