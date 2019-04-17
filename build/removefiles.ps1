function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Get-ChildItem -Path ./Piraeus.SiloHost/Output -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ./Piraeus.WebApi/Output -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ./Piraeus.WebSocketGateway/Output -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ./Piraeus.SiloHost-Out -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ./Piraeus.WebApi-Out -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ./Piraeus.WebSocketGateway-Out -Recurse | Remove-Item -force -recurse


