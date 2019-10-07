msbuild ..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.SiloHost\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.SiloHost.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.SiloHost_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.SiloHost.log

msbuild ..\src\Piraeus.WebApi\Piraeus.WebApi.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.WebApi\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebApi.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebApi_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebApi.log

msbuild ..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.WebSocketGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log_errors;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebSocketGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log


dotnet publish "..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj" -c Release -o "..\..\build\Piraeus.SiloHost-Out"

dotnet publish "..\src\Piraeus.WebApi\Piraeus.WebApi.csproj" -c Release -o "..\..\build\Piraeus.WebApi-Out"

dotnet publish "..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj" -c Release -o "..\..\build\Piraeus.WebSocketGateway-Out"


docker rmi piraeus-silo
docker rmi piraeus-mgmt-api
docker rmi piraeus-websocket-gateway

docker rmi skunklab/piraeus-silo
docker rmi skunklab/piraeus-mgmt-api
docker rmi skunklab/piraeus-websocket-gateway

docker build -t piraeus-silo ./Piraeus.SiloHost-Out
docker build -t piraeus-mgmt-api ./Piraeus.WebApi-Out
docker build -t piraeus-websocket-gateway ./Piraeus.WebSocketGateway-Out


docker tag piraeus-silo skunklab/piraeus-silo
docker tag piraeus-mgmt-api skunklab/piraeus-mgmt-api
docker tag piraeus-websocket-gateway skunklab/piraeus-websocket-gateway

docker push skunklab/piraeus-silo
docker push skunklab/piraeus-mgmt-api
docker push skunklab/piraeus-websocket-gateway







