# Imagen de despliegue para Nexit.API (backend de Nexit).
# Build multi-stage: compila con el SDK completo y la imagen final solo
# lleva el runtime de ASP.NET (mucho más liviana y con menos superficie
# de ataque). Funciona igual en Railway, Render, Fly.io, DigitalOcean App
# Platform o cualquier otro que soporte "despliega este Dockerfile".

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia solo los .csproj primero para aprovechar el cache de Docker en
# `dotnet restore` -- si no cambian las dependencias, este paso no se
# vuelve a correr aunque cambie el código.
COPY src/Nexit.Core/Nexit.Core.csproj src/Nexit.Core/
COPY src/Nexit.Application/Nexit.Application.csproj src/Nexit.Application/
COPY src/Nexit.Infrastructure/Nexit.Infrastructure.csproj src/Nexit.Infrastructure/
COPY src/Nexit.API/Nexit.API.csproj src/Nexit.API/
RUN dotnet restore src/Nexit.API/Nexit.API.csproj

COPY src/ src/
RUN dotnet publish src/Nexit.API/Nexit.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# La mayoría de plataformas (Railway, Render, Fly.io, DO) inyectan la
# variable de entorno PORT y esperan que la app escuche ahí -- si no,
# el healthcheck de la plataforma falla y el despliegue no arranca.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Nexit.API.dll"]
