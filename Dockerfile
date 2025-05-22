FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

# Restore dependencies berdasarkan file csproj utama
RUN dotnet restore EchoBot.csproj

# Publish dengan nama file csproj yang sesuai
RUN dotnet publish EchoBot.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EchoBot.dll"]