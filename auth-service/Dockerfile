# Usa el SDK de .NET para poder construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar el archivo .csproj primero para optimizar la capa de caché de Docker
COPY ["AuthService.csproj", "./"]

# Restaurar las dependencias
RUN dotnet restore

# Copiar el código fuente completo y construir la aplicación
COPY . .
RUN dotnet publish -c Release -o /app

# Usa la imagen de runtime de .NET para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "AuthService.dll"]
