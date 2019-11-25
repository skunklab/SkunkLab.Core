FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS runtime
WORKDIR /app
EXPOSE 11111
EXPOSE 30000

ENTRYPOINT ["dotnet", "Piraeus.SiloHost.dll"]
COPY . /app

