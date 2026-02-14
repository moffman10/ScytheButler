FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app
COPY ScytheButler/ScytheButler.csproj ./ScytheButler/
RUN dotnet restore ./ScytheButler/ScytheButler.csproj
COPY . ./
RUN dotnet publish ./ScytheButler/ScytheButler.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "ScytheButler.dll"]
