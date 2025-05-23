# FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
# WORKDIR /app

# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
# WORKDIR /src
# COPY . .

# # Restore dependencies berdasarkan file csproj utama
# RUN dotnet restore EchoBot.csproj

# # Publish dengan nama file csproj yang sesuai
# RUN dotnet publish EchoBot.csproj -c Release -o /app/publish

# FROM base AS final
# WORKDIR /app
# COPY --from=build /app/publish .
# ENTRYPOINT ["dotnet", "EchoBot.dll"]

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Salin semua file
COPY . .

# Restore dependencies
RUN dotnet restore EchoBot.csproj

# Publish aplikasi
RUN dotnet publish EchoBot.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Expose port 80 agar Railway bisa forward
EXPOSE 80

# Biarkan ASP.NET listen ke semua interface di port 80
ENV ASPNETCORE_URLS=http://+:80

# Salin hasil publish dari stage build
COPY --from=build /app/publish .

# Jalankan aplikasi
ENTRYPOINT ["dotnet", "EchoBot.dll"]
