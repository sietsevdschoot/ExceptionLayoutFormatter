[CmdletBinding()]
param(
	[switch] $pushToNuget
)

$solutionRoot = Join-Path $PSScriptRoot "..\..\" -Resolve

$credentialFile = [IO.FileInfo]"$solutionRoot\..\Credentials\ExceptionLayoutFormatterNuget.txt"

if ($credentialFile.Exists) {

    $nugetApiKey = (Get-Content $credentialFile.FullName -Raw).Trim()
}
elseif (!$credentialFile.Exists -and $pushToNuget) {
    
    throw "Can't find credentialFile '$($credentialFile.FullName)'"    
}

Function Get-ProjectInfo {
    param(
        [IO.FileInfo] $projectFile
    ) 

    $nuspecData = ([xml](Get-Content $projectFile)).Project.PropertyGroup

    $packageId = if ($nuspecData.PackageId) { $nuspecData.PackageId } else { $nuspecData.AssemblyName }

    $description = $nuspecData.Description

    $projectDll = (Get-ChildItem "$($projectFile.Directory.FullName)\bin\*.dll" -recurse | Sort-Object -prop CreationTime -Descending)[0]

    $projectHash = (Get-FileHash -Algorithm MD5 -Path $projectDll).Hash

    [PsCustomObject]@{
        PackageId = $packageId;
        Description = $description;
        Hash = $projectHash;
    }    
}

function Get-PackageInfoFromFeed {
    param(
        [string] $packageId
    )

    $packageFeed = "https://api-v2v3search-0.nuget.org/query?q=packageid:$packageId"

    $response = Invoke-RestMethod -Uri $packageFeed -Verbose:$false

    if ($null -eq $response.data) {
    
        Throw "No packages found.."
    }

    [PsCustomObject]@{ `
        PackageId = $response.data.id; `
        Version = $response.data.version; `
        Description = $response.data.description; `
    }
}

Function SkipOrPushNewPackage {
    param (
        [IO.FileInfo] $projectFile
    )

    $projectInfo = Get-ProjectInfo $projectFile
    $publishedPackage = Get-PackageInfoFromFeed -packageId $projectInfo.PackageId

    $hasPackageChanged = $true

    if ($publishedPackage) {

        $publishedPackageHash = $null
        if ($publishedPackage.Description.Contains("|")) 
        {
            $publishedPackageHash = ($publishedPackage.Description.Split("|") | Select-Object -Last 1).Trim()
        }
        
        #if the hash no longer matches, we should push a new package
        $hasPackageChanged = ($projectInfo.Hash -ne $publishedPackageHash)
    }

    if ($hasPackageChanged) {

        PublishNewPackage -projectInfo $projectInfo -publishedPackage $publishedPackage
    }
    else {
    
        Write-Host " [SKIP] $($projectInfo.PackageId) has not changed"
    }
} 

Function PublishNewPackage {
    param (
        $projectInfo,
        $publishedPackage
    )

    $newVersion = Get-NewPackageVersionNumber -packageId ($projectInfo.PackageId) -currentVersion $($publishedPackage.Version)

    $newDescription = ("{0} | {1}" -f $projectInfo.Description, $projectInfo.Hash) 

    dotnet pack $($projectFile.FullName)`
        /p:PackageVersion=$newVersion `
		/p:Description=$newDescription `
		--no-build `
		--output $solutionRoot\build | Out-String | Write-Verbose
        
    $isSuccess = $LASTEXITCODE -eq 0

    $generatedPackage = [IO.Path]::Combine($solutionRoot,"build","$($projectInfo.PackageId).$newVersion.nupkg")
    
    if ($nugetApiKey -and $pushToNuget) {

        dotnet nuget push $generatedPackage -s https://api.nuget.org/v3/index.json -k $nugetApiKey | Out-String | Write-Verbose
        
        $isSuccess = $isSuccess -and $LASTEXITCODE -eq 0
        
        if ($isSuccess) {
        
            Write-Host " [DONE] Pushed $($projectInfo.PackageId) $newVersion $(($newDescription.Split('|') | Select-Object -Last 1).Trim()) to NuGet Feed"  
        }
        else {
        
            Write-Host " [FAIL] Creating / Pushing $($projectInfo.PackageId) $newVersion $(($newDescription.Split('|') | Select-Object -Last 1).Trim()) "  
        }
    }
    elseif ($isSuccess) {
    
        Write-Host " [DONE] Created $generatedPackage" 
    }
}

Function Get-NewPackageVersionNumber {
    param (
		[string] $packageId,
        [string] $currentVersion
    )

    # Configure major and minor version increment bumps
    $initialVersions = @{

        "ExceptionFormatter" = "1.0.0.0";
    }

    if ($currentVersion) {
    
        $version = [version]$currentVersion
    }
    else {
        
        $version = [version] $(if ($initialVersions.ContainsKey($packageId)) { $initialVersions.($packageId) } else { "1.0.0.0" })
    }
        
    if (([version]$initialVersions.($packageId)) -gt $version) {
    
        $newVersion = [version]$initialVersions.($packageId)
    }
    else {
    
        $newVersion = ("{0}.{1}.{2}.{3}" -f $version.Major, $version.Minor, $version.Build, ([Math]::Max($version.Revision, 0) + 1))
    }

    $newVersion
}

SkipOrPushNewPackage "$solutionRoot\src\ExceptionLayoutFormatter\ExceptionLayoutFormatter.csproj"