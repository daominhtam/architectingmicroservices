FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 8084
#EXPOSE 443

#RUN ["dir *.*"]

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY ["src/ApiGateway/ApiGateway.API.csproj", "ApiGateway/"]
COPY ["src/BuildingBlocks/RestCommunication/RestCommunication.csproj", "BuildingBlocks/RestCommunication/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]
COPY ["src/BuildingBlocks/ServiceDiscovery/ServiceDiscovery.csproj", "BuildingBlocks/ServiceDiscovery/"]
COPY ["src/BuildingBlocks/Snowflake/Snowflake.csproj", "BuildingBlocks/Snowflake/"]
COPY ["src/BuildingBlocks/Utilities/Utilities.csproj", "BuildingBlocks/Utilities/"]
COPY ["ArchitectingMicroservices.sln", "./"]

RUN dotnet restore "ApiGateway/ApiGateway.API.csproj"

COPY . .

WORKDIR "/src/ApiGateway"

RUN dotnet build -c Release -o /app/build


# Pulls Dotnet Image from Docker Hub
##FROM microsoft/dotnet:2.0.0-sdk-2.0.2-nanoserver
# Creates a Working Directory
##WORKDIR dockerdemo
#Add all the required applications artifacts inside the container
##ADD . .
#Command to Run when container is up and running
##ENTRYPOINT ["dotnet","bin/Debug/netcoreapp2.0/publish/ApiGateway.API.dll"]
# Exposes the Container Port
##EXPOSE 8084
##ENV ASPNETCORE_URLS http://0.0.0.0:8084
