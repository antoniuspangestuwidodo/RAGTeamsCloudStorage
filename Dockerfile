FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Salin semua file project
COPY . .

# Restore dependencies
RUN dotnet restore

# Publish project utama (ganti EchoBot.csproj sesuai nama file kamu)
RUN dotnet publish EchoBot.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Jalankan aplikasi
ENTRYPOINT ["dotnet", "EchoBot.dll"]
