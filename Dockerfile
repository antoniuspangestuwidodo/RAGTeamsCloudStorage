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

# Copy all files
COPY . .

# Restore dependencies
RUN dotnet restore EchoBot.csproj

# Publish application
RUN dotnet publish EchoBot.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Expose port 80 for Railway forward
EXPOSE 80

# Allowing ASP.NET listen to all  interface in port 80
ENV ASPNETCORE_URLS=http://+:80

# Copy publish result from stage build
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "EchoBot.dll"]
