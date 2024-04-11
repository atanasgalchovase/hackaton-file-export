#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["hackaton-oauth.api/hackaton-oauth.api.csproj", "hackaton-oauth.api/"]
COPY ["hackaton-oauth.common/hackaton-oauth.common.csproj", "hackaton-oauth.common/"]
COPY ["hackaton-oauth.data/hackaton-oauth.data.csproj", "hackaton-oauth.data/"]
COPY ["hackaton-oauth.services/hackaton-oauth.services.csproj", "hackaton-oauth.services/"]
RUN dotnet restore "./hackaton-oauth.api/hackaton-oauth.api.csproj"
COPY . .
WORKDIR "/src/hackaton-oauth.api"
RUN dotnet build "./hackaton-oauth.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./hackaton-oauth.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hackaton-oauth.api.dll"]