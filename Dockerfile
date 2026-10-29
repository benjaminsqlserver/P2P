FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so the layer caches when only source changes.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/P2P.Domain/*.csproj          src/P2P.Domain/
COPY src/P2P.Contracts/*.csproj       src/P2P.Contracts/
COPY src/P2P.Application/*.csproj     src/P2P.Application/
COPY src/P2P.Infrastructure/*.csproj  src/P2P.Infrastructure/
COPY src/P2P.Web/*.csproj             src/P2P.Web/
COPY src/P2P.Web.Client/*.csproj      src/P2P.Web.Client/
RUN dotnet restore src/P2P.Web/P2P.Web.csproj

COPY . .
RUN dotnet publish src/P2P.Web/P2P.Web.csproj -c Release -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "P2P.Web.dll"]
