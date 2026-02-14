# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the csproj and restore dependencies
COPY ScytheButler/ScytheButler.csproj ./ScytheButler/
RUN dotnet restore ./ScytheButler/ScytheButler.csproj

# Copy all files and build
COPY . ./
RUN dotnet publish ./ScytheButler/ScytheButler.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose the port Fly.io expects
EXPOSE 8080

ENTRYPOINT ["dotnet", "ScytheButler.dll"]
