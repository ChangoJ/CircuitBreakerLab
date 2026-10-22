ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props CircuitBreakerLab.slnx ./
COPY src/CircuitBreakerLab.Domain/CircuitBreakerLab.Domain.csproj src/CircuitBreakerLab.Domain/
COPY src/CircuitBreakerLab.Application/CircuitBreakerLab.Application.csproj src/CircuitBreakerLab.Application/
COPY src/CircuitBreakerLab.Infrastructure/CircuitBreakerLab.Infrastructure.csproj src/CircuitBreakerLab.Infrastructure/
COPY src/CircuitBreakerLab.Api/CircuitBreakerLab.Api.csproj src/CircuitBreakerLab.Api/
RUN dotnet restore src/CircuitBreakerLab.Api/CircuitBreakerLab.Api.csproj

COPY src/ src/
RUN dotnet publish src/CircuitBreakerLab.Api/CircuitBreakerLab.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CircuitBreakerLab.Api.dll"]
