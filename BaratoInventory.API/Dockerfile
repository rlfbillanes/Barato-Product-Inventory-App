FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for caching restore layer
COPY ["BaratoInventory.Core/BaratoInventory.Core.csproj", "BaratoInventory.Core/"]
COPY ["BaratoInventory.Infrastructure/BaratoInventory.Infrastructure.csproj", "BaratoInventory.Infrastructure/"]
COPY ["BaratoInventory.API/BaratoInventory.API.csproj", "BaratoInventory.API/"]

RUN dotnet restore "BaratoInventory.API/BaratoInventory.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/BaratoInventory.API"
RUN dotnet publish "BaratoInventory.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BaratoInventory.API.dll"]
