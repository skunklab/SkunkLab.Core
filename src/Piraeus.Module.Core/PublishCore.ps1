
$Path = '.\PiraeusCore'
$ModuleName = 'PiraeusCore'
$Author = 'SkunkLab'
$Description = 'PowerShell Core module for Piraeus Management API.'

New-ModuleManifest -Path $Path\$ModuleName.psd1 -RootModule $ModuleName.psm1 -Description $Description -Author $Author -ModuleVersion "1.1.1"

Set-Content -Value '' -Path "$Path\$ModuleName.psm1"


$PublishParams = @{

    NuGetApiKey = 'oy2exrhnc6qrx6jwcplapzpkgq22vr7epdj23c4unluyji' 

    Path = $Path

    ProjectUri = 'https://github.com/skunklab'

    Tags = @('Piraeus', 'REST', 'API' )

}

Publish-Module @PublishParams