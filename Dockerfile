# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copy the .csproj file from the root
COPY ["HolyWater.Server.csproj", "./"]

# 2. Restore dependencies
RUN dotnet restore "HolyWater.Server.csproj"

# 3. Copy everything else from the root
COPY . .

# 4. Build and Publish
RUN dotnet build "HolyWater.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HolyWater.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HolyWater.Server.dll"]