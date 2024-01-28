# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy the Common project files and restore
COPY ../Common/Common.csproj ../Common/
RUN dotnet restore ../Common/Common.csproj

# Copy the Server project files and restore
COPY Server.csproj .
RUN dotnet restore

# Copy the Common source and build
COPY ../Common/ ../Common/
RUN dotnet build ../Common/Common.csproj -c Release -o /out/Common

# Copy the Server source and publish
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Copy the Networking.Server.json configuration file from the Resources folder
COPY --from=build-env /app/Resources/Networking.Server.json ./c.json

# Set the entry point to run the Server application
ENTRYPOINT ["dotnet", "YourServerExecutableName.dll"]
