# Usa la imagen oficial de .NET 8 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia csproj y restaura dependencias
COPY src/webProductos.Api/*.csproj ./webProductos.Api/
COPY src/webProductos.Application/*.csproj ./webProductos.Application/
COPY src/webProductos.Domain/*.csproj ./webProductos.Domain/
COPY src/webProductos.Infrastructure/*.csproj ./webProductos.Infrastructure/
RUN dotnet restore ./webProductos.Api/webProductos.Api.csproj

# Copia todo el código
COPY src/. ./ 

# Publica en modo Release
RUN dotnet publish ./webProductos.Api/webProductos.Api.csproj -c Release -o /app/publish

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Puerto expuesto
EXPOSE 8080

# Comando para ejecutar la API
ENTRYPOINT ["dotnet", "webProductos.Api.dll"]
