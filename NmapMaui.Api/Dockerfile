FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Nmap ve Gobuster gibi sistem araçlarını container içine kuruyoruz
# API remote scan isteklerini (RunProcessAsync ile nmap/gobuster) yerine getirebilsin diye
RUN apt-get update && apt-get install -y --no-install-recommends \
    nmap \
    gobuster \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["NmapMaui.Api.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NmapMaui.Api.dll"]
