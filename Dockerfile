# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY AgendaDentista.Dominio/AgendaDentista.Dominio.csproj AgendaDentista.Dominio/
COPY AgendaDentista.Aplicacion/AgendaDentista.Aplicacion.csproj AgendaDentista.Aplicacion/
COPY AgendaDentista.Infraestructura/AgendaDentista.Infraestructura.csproj AgendaDentista.Infraestructura/
COPY AgendaDentista.API/AgendaDentista.API.csproj AgendaDentista.API/
RUN dotnet restore AgendaDentista.API/AgendaDentista.API.csproj

# Copiar todo el codigo y publicar
COPY . .
RUN dotnet publish AgendaDentista.API/AgendaDentista.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AgendaDentista.API.dll"]
