#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Module05/Module05.DockerWebAPI/Module05.DockerWebAPI.csproj", "Module05/Module05.DockerWebAPI/"]
RUN dotnet restore "Module05/Module05.DockerWebAPI/Module05.DockerWebAPI.csproj"
COPY . .
WORKDIR "/src/Module05/Module05.DockerWebAPI"
RUN dotnet build "Module05.DockerWebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Module05.DockerWebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Module05.DockerWebAPI.dll"]