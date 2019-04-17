FROM microsoft/dotnet:2.2-runtime
WORKDIR /app
EXPOSE 11111
EXPOSE 30000

ENTRYPOINT ["dotnet", "Piraeus.SiloHost.dll"]
COPY . /app

