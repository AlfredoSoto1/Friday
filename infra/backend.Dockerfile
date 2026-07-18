FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

WORKDIR /src

COPY backend/Friday.Backend.csproj backend/
RUN dotnet restore backend/Friday.Backend.csproj

COPY backend/ backend/
RUN dotnet publish backend/Friday.Backend.csproj \
  --configuration Release \
  --output /app/publish \
  --no-restore \
  /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS runtime

WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=5
ENV DOTNET_gcServer=0

EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "Friday.Backend.dll"]
