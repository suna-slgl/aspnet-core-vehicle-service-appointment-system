# Stage 1: Build the application
# Use the .NET 9 SDK image as a base for building the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution and project files to restore dependencies first
# This leverages Docker's layer caching. Dependencies are only re-downloaded if these files change.
COPY ["aspnet-core-vehicle-service-appointment-system.sln", "."]
COPY ["src/VehicleServiceApp/VehicleServiceApp.csproj", "src/VehicleServiceApp/"]
RUN dotnet restore "aspnet-core-vehicle-service-appointment-system.sln"

# Copy the rest of the source code
COPY . .

# Build the project in Release configuration
WORKDIR "/src/src/VehicleServiceApp"
RUN dotnet build "VehicleServiceApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "VehicleServiceApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create the final runtime image
# Use the smaller ASP.NET 9 runtime image for the final application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Render dynamically assigns a port, so we don't need to hardcode it.
# The ASPNETCORE_URLS environment variable will be set by Render.
# We expose port 80 as a standard. Render will map its internal port to this.
EXPOSE 80

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "VehicleServiceApp.dll"]
