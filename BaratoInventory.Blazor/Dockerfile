FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for caching restore layer
COPY ["BaratoInventory.Core/BaratoInventory.Core.csproj", "BaratoInventory.Core/"]
COPY ["BaratoInventory.Blazor/BaratoInventory.Blazor.csproj", "BaratoInventory.Blazor/"]

RUN dotnet restore "BaratoInventory.Blazor/BaratoInventory.Blazor.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/BaratoInventory.Blazor"
RUN dotnet publish "BaratoInventory.Blazor.csproj" -c Release -o /app/publish

# Static Web Hosting via Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY BaratoInventory.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
