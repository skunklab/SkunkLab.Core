msbuild ..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.SiloHost\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.SiloHost_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.SiloHost_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.SiloHost.log

msbuild ..\src\Piraeus.WebApi\Piraeus.WebApi.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.WebApi\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebApi_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebApi_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebApi.log

msbuild ..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.WebSocketGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.WebSocketGateway_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.WebSocketGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.WebSocketGateway.log

msbuild ..\src\Piraeus.TcpGateway\Piraeus.TcpGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.TcpGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.TcpGateway_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.TcpGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.TcpGateway.log

msbuild ..\src\Piraeus.UdpGateway\Piraeus.UdpGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.UdpGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.UdpGateway_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.UdpGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.UdpGateway.log

msbuild ..\src\Piraeus.HttpGateway\Piraeus.HttpGateway.csproj /t:Clean,Rebuild,restore /p:OutputPath=..\..\build\Piraeus.HttpGateway\Output /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=.\BuildOutput\Piraeus.HttpGateway_errors.log;errorsonly /flp2:logfile=.\BuildOutput\Piraeus.HttpGateway_warnings.log;warningsonly /flp3:logfile=.\BuildOutput\Piraeus.HttpGateway.log

dotnet publish "..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj" -c Release -o "Piraeus.SiloHost-Out"
dotnet publish "..\src\Piraeus.WebApi\Piraeus.WebApi.csproj" -c Release -o "Piraeus.WebApi-Out"
dotnet publish "..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj" -c Release -o "Piraeus.WebSocketGateway-Out"
dotnet publish "..\src\Piraeus.TcpGateway\Piraeus.TcpGateway.csproj" -c Release -o "Piraeus.TcpGateway-Out"
dotnet publish "..\src\Piraeus.UdpGateway\Piraeus.UdpGateway.csproj" -c Release -o "Piraeus.UdpGateway-Out"
dotnet publish "..\src\Piraeus.HttpGateway\Piraeus.HttpGateway.csproj" -c Release -o "Piraeus.HttpGateway-Out"



SET nameId=""

for /f %%i in ('docker images -q piraeus-silo:v2.0') DO SET nameId=%%i

ECHO %nameId%

IF %nameId% == "" (
    ECHO not found
) ELSE (
    ECHO found
    SET nameId=""
)


CALL :func "piraeus-silo"
CALL :func "piraeus-mgmt-api"
CALL :remove "piraeus-websocket-gateway"
CALL :remove "piraeus-tcp-gateway"
CALL :remove "piraeus-udp-gateway"
CALL :remove "piraeus-http-gateway"
CALL :remove "piraeus-silo:v3.0"
CALL :remove "piraeus-mgmt-api:v3.0"
CALL :remove "piraeus-websocket-gateway:v3.0"
CALL :remove "piraeus-tcp-gateway:v3.0"
CALL :remove "piraeus-udp-gateway:v3.0"
CALL :remove "piraeus-http-gateway:v3.0"
CALL :remove "skunklab/piraeus-silo:v3.0"
CALL :remove "skunklab/piraeus-mgmt-api:v3.0"
CALL :remove "skunklab/piraeus-websocket-gateway:v3.0"
CALL :remove "skunklab/piraeus-tcp-gateway:v3.0"
CALL :remove "skunklab/piraeus-udp-gateway:v3.0"
CALL :remove "skunklab/piraeus-http-gateway:v3.0"



docker build -t piraeus-silo ./Piraeus.SiloHost-Out
docker build -t piraeus-mgmt-api ./Piraeus.WebApi-Out
docker build -t piraeus-websocket-gateway ./Piraeus.WebSocketGateway-Out
docker build -t piraeus-tcp-gateway ./Piraeus.TcpGateway-Out
docker build -t piraeus-udp-gateway ./Piraeus.UdpGateway-Out
docker build -t piraeus-http-gateway ./Piraeus.HttpGateway-Out


docker tag piraeus-silo piraeus-silo:v3.0
docker tag piraeus-mgmt-api piraeus-mgmt-api:v3.0
docker tag piraeus-websocket-gateway piraeus-websocket-gateway:v3.0
docker tag piraeus-tcp-gateway piraeus-tcp-gateway:v3.0
docker tag piraeus-udp-gateway piraeus-udp-gateway:v3.0
docker tag piraeus-http-gateway piraeus-http-gateway:v3.0

docker tag piraeus-silo:v3.0 skunklab/piraeus-silo:v3.0
docker tag piraeus-mgmt-api:v3.0 skunklab/piraeus-mgmt-api:v3.0
docker tag piraeus-websocket-gateway:v3.0 skunklab/piraeus-websocket-gateway:v3.0
docker tag piraeus-tcp-gateway:v3.0 skunklab/piraeus-tcp-gateway:v3.0
docker tag piraeus-udp-gateway:v3.0 skunklab/piraeus-udp-gateway:v3.0
docker tag piraeus-http-gateway:v3.0 skunklab/piraeus-http-gateway:v3.0


::docker push skunklab/piraeus-silo:v3.0
::docker push skunklab/piraeus-mgmt-api:v3.0
::docker push skunklab/piraeus-websocket-gateway:v3.0
::docker push skunklab/piraeus-tcp-gateway:v3.0
::docker push skunklab/piraeus-udp-gateway:v3.0
::docker push skunklab/piraeus-http-gateway:v3.0


ECHO "done"
EXIT /B %ERRORLEVEL%

:remove
SET nameId=""
for /f %%i in ('docker images -q "%~1"') DO SET nameId=%%i

ECHO " %nameId% "

IF %nameId% == "" (
    ECHO not found
) ELSE (
    ECHO removing image "%~1"
    docker rmi %~1
    SET nameId=""
    SET image=""
)
EXIT /B 0



