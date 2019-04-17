function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Get-ChildItem -Path ./Piraeus.SiloHost/Output -Recurse | Remove-Item -force -recurse

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj /p:OutputPath=..\..\build\Piraeus.SiloHost\Output /p:Configuration=Release /flp1:logfile=.\BuildOutput\Piraeus.SiloHost.log;warningsonly"
Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\src\Piraeus.WebApi\Piraeus.WebApi.csproj /p:OutputPath=..\..\build\Piraeus.WebApi\Output /p:Configuration=Release /flp1:logfile=.\BuildOutput\Piraeus.WebApi.log;warningsonly"
Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj /p:OutputPath=..\..\build\Piraeus.WebSocketGateway\Output /p:Configuration=Release /flp1:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log;warningsonly"







