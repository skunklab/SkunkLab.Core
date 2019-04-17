FROM microsoft/dotnet:2.2.2-runtime AS build-env
WORKDIR /app

COPY ./Capl.Core/*.csproj ./
#RUN dotnet restore

#COPY . ./
RUN dotnet publish -c Release -o out
