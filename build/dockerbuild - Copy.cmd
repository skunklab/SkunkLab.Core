powershell ./removefiles.ps1

msbuild ..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\..\build\Piraeus.SiloHost\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.SiloHost.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.SiloHost_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.SiloHost.log

msbuild ..\src\Piraeus.WebApi\Piraeus.WebApi.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\..\build\Piraeus.WebApi\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebApi.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebApi_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebApi.log

msbuild ..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\..\build\Piraeus.WebSocketGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebSocketGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log

msbuild ..\src\Piraeus.TcpGateway\Piraeus.TcpGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\..\build\Piraeus.TcpGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.TcpGateway.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.TcpGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.TcpGateway.log

msbuild ..\src\Piraeus.HttpGateway\Piraeus.HttpGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\..\build\Piraeus.HttpGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.HttpGateway.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.HttpGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.HttpGateway.log


docker build -t orleans-silo ./Piraeus.SiloHost
docker build -t webapi ./Piraeus.WebApi
docker build -t websocketgateway ./Piraeus.WebSocketGateway
docker build -t tcpgateway ./Piraeus.TcpGateway


