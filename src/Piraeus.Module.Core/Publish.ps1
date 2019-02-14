
$Path = '.\PiraeusCore'
$ModuleName = 'PiraeusCore'
$Author = 'SkunkLab'
$Description = 'PowerShell Core module for Piraeus Management API.'

$manifestSplat = @{
        Path              = ".\PiraeusCore\PiraeusCore.psd1"
        Author            = 'SkunkLab'
        ModuleVersion     = "0.9.1"
        NestedModules     = @('.bin\Release\netcoreapp2.1\Piraeus.Module.Core.dll')
        RootModule        = "PiraeusCore.psm1"
    }
    New-ModuleManifest @manifestSplat

Set-Content -Value '' -Path ".\PiraeusCore\PiraeusCore.psm1"



New-ModuleManifest -Path $Path\$ModuleName.psd1 -RootModule $ModuleName.psm1 -Description $Description -Author $Author -ModuleVersion "1.1.0"


$PublishParams = @{

    NuGetApiKey = 'oy2exrhnc6qrx6jwcplapzpkgq22vr7epdj23c4unluyji' 

    Path = $Path

    ProjectUri = 'https://github.com/skunklab'

    Tags = @('PiraeusCore', 'Piraeus', 'REST', 'API' )

}

Publish-Module @PublishParams