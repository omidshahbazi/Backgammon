FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Server/Server.csproj", "Server/"]
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Server/Server.csproj"
COPY . .

WORKDIR "/src/Server"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Server/Resources/Resources /app/Resources 
COPY Server/Resources/Network /app/ 
ENTRYPOINT ["dotnet", "Server.dll"]
EXPOSE 80