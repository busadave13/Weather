# Base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Install curl for health checks (Git only needed for Production environment)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Weather/Weather.csproj", "Weather/"]
RUN dotnet restore "Weather/Weather.csproj"
COPY src/Weather/. Weather/
WORKDIR "/src/Weather"
RUN dotnet build "Weather.csproj" -c Release -o /app/build
# Publish image
FROM build AS publish
RUN dotnet publish "Weather.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for Git repository clone
RUN mkdir -p /app/mocks

ENTRYPOINT ["dotnet", "Weather.dll"]
