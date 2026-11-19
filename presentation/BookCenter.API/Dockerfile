# Build context: the repository root (see compose.yaml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first: this layer stays cached until a project file changes.
COPY src/Core/BookCenter.Domain/BookCenter.Domain.csproj src/Core/BookCenter.Domain/
COPY src/Core/BookCenter.Application/BookCenter.Application.csproj src/Core/BookCenter.Application/
COPY src/Infrastructure/BookCenter.Infrastructure/BookCenter.Infrastructure.csproj src/Infrastructure/BookCenter.Infrastructure/
COPY src/Infrastructure/BookCenter.Persistence/BookCenter.Persistence.csproj src/Infrastructure/BookCenter.Persistence/
COPY presentation/BookCenter.API/BookCenter.API.csproj presentation/BookCenter.API/
RUN dotnet restore presentation/BookCenter.API/BookCenter.API.csproj

COPY src/ src/
COPY presentation/ presentation/
RUN dotnet publish presentation/BookCenter.API/BookCenter.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Run as the image's built-in non-root user.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookCenter.API.dll"]
