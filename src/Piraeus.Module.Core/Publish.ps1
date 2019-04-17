
$Path = '.\PiraeusModuleCore'
$ModuleName = 'PiraeusModuleCore'
$Author = 'SkunkLab'
$Description = 'PowerShell Core module for Piraeus Management API.'

$manifestSplat = @{
        Path              = ".\PiraeusCore.psd1"
        Author            = 'SkunkLab'
        ModuleVersion     = "1.1.11"
        NestedModules     = @('.bin\Release\netcoreapp2.2\Piraeus.Module.Core.dll')
        RootModule        = "PiraeusModuleCore"
    }
    New-ModuleManifest @manifestSplat

#Set-Content -Value '' -Path ".\PiraeusModuleCore\PiraeusModuleCore.psm1"

Write-Host $Path
Write-Host $ModuleName.ps1

New-ModuleManifest -Path $Path\$ModuleName.psd1 -RootModule $ModuleName.psm1 -Description $Description -Author $Author -ModuleVersion "1.1.11"


$PublishParams = @{

    NuGetApiKey = 'oy2exrhnc6qrx6jwcplapzpkgq22vr7epdj23c4unluyji' 

    Path = $Path

    ProjectUri = 'https://github.com/skunklab'

    Tags = @('PiraeusCore', 'PiraeusModuleCore', 'Piraeus', 'Management','REST', 'API' )

}

Publish-Module @PublishParams