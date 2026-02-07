# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY Bridgo.csproj .
RUN dotnet restore -r linux-x64

# Copy everything and publish
COPY . .
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish -p:RuntimeIdentifier=linux-x64

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Install cultures for localization
RUN apt-get update && apt-get install -y --no-install-recommends locales && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Cloud Run uses PORT 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Bridgo.dll"]
