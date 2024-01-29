FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
RUN dotnet restore "Common/Common.csproj"
COPY . .
WORKDIR "/src/Common"
RUN dotnet build "Common.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Common.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Common.dll"]
