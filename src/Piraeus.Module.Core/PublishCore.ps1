
$Path = '.\PiraeusModuleCore'
$ModuleName = 'PiraeusModuleCore'
$Author = 'SkunkLab'
$Description = 'PowerShell Core module for Piraeus Management API.'

#New-ModuleManifest -Path $Path\$ModuleName.psd1 -RootModule $ModuleName.psm1 -Description $Description -Author $Author -ModuleVersion "1.1.12"

Write-Host $Path

Set-Content -Value '' -Path "$Path\$ModuleName.psm1"


$PublishParams = @{

    NuGetApiKey = 'oy2exrhnc6qrx6jwcplapzpkgq22vr7epdj23c4unluyji' 

    Path = $Path

    ProjectUri = 'https://github.com/skunklab'

    Tags = @('PiraeusCore', 'PiraeusModuleCore', 'Piraeus', 'Management','REST', 'API' )

}

Publish-Module @PublishParams