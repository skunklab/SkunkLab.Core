
dotnet build "..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj" -c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.SiloHosts" --force
dotnet build "..\src\Piraeus.WebApi\Piraeus.WebApi.csproj" c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.WebApi" --force
dotnet build "..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj" -c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.WebSocketGateway" --force
dotnet build "..\src\Piraeus.TcpGateway\Piraeus.TcpGateway.csproj" -c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.TcpGateway" --force
dotnet build "..\src\Piraeus.UdpGateway\Piraeus.UdpGateway.csproj" -c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.UdpGateway" --force
dotnet build "..\src\Piraeus.HttpGateway\Piraeus.HttpGateway.csproj" -c Release -f netcoreapp3.0 -v n -o "./BuildOutput/Piraeus.HttpGateway" --force

dotnet publish "..\src\Piraeus.SiloHost\Piraeus.SiloHost.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.SiloHost" 
dotnet publish "..\src\Piraeus.WebApi\Piraeus.WebApi.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.WebApi" 
dotnet publish "..\src\Piraeus.WebSocketGateway\Piraeus.WebSocketGateway.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.WebSocketGateway" 
dotnet publish "..\src\Piraeus.TcpGateway\Piraeus.TcpGateway.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.TcpGateway" 
dotnet publish "..\src\Piraeus.UdpGateway\Piraeus.UdpGateway.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.UdpGateway" 
dotnet publish "..\src\Piraeus.HttpGateway\Piraeus.HttpGateway.csproj" -c Release -f netcoreapp3.0 -o "Piraeus.HttpGateway" 

docker rmi skunklab/piraeus-silo:v3.0
docker rmi skunklab/piraeus-mgmt-api:v3.0
docker rmi skunklab/piraeus-websocket-gateway:v3.0
docker rmi skunklab/piraeus-tcp-gateway:v3.0
docker rmi skunklab/piraeus-udp-gateway:v3.0
docker rmi skunklab/piraeus-http-gateway:v3.0


docker build -t skunklab/piraeus-silo:v3.0 ./Piraeus.SiloHost
docker build -t skunklab/piraeus-mgmt-api:v3.0 ./Piraeus.WebApi
docker build -t skunklab/piraeus-websocket-gateway:v3.0 ./Piraeus.WebSocketGateway
docker build -t skunklab/piraeus-tcp-gateway:v3.0 ./Piraeus.TcpGateway
docker build -t skunklab/piraeus-udp-gateway:v3.0 ./Piraeus.UdpGateway
docker build -t skunklab/piraeus-http-gateway:v3.0 ./Piraeus.HttpGateway


::docker push skunklab/piraeus-silo:v3.0
::docker push skunklab/piraeus-mgmt-api:v3.0
::docker push skunklab/piraeus-websocket-gateway:v3.0
::docker push skunklab/piraeus-tcp-gateway:v3.0
::docker push skunklab/piraeus-udp-gateway:v3.0
::docker push skunklab/piraeus-http-gateway:v3.0


