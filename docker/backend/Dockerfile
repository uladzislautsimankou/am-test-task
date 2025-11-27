FROM mcr.microsoft.com/dotnet/sdk:10.0 AS prepare-restore
WORKDIR /src

# Копируем csproj
COPY src/backend/AM.TestTask.Api/AM.TestTask.Api.csproj AM.TestTask.Api/
COPY src/backend/AM.TestTask.Business/AM.TestTask.Business.csproj AM.TestTask.Business/
COPY src/backend/AM.TestTask.Data.Relational.AppDatabase.PostgreSql/AM.TestTask.Data.Relational.AppDatabase.PostgreSql.csproj AM.TestTask.Data.Relational.AppDatabase.PostgreSql/
COPY src/backend/AM.TestTask.Infrastructure/AM.TestTask.Infrastructure.csproj AM.TestTask.Infrastructure/
COPY src/backend/AM.TestTask.Data.Relational.Abstractions/AM.TestTask.Data.Relational.Abstractions.csproj AM.TestTask.Data.Relational.Abstractions/
COPY src/backend/AM.TestTask.Data.Relational.EntityFramework/AM.TestTask.Data.Relational.EntityFramework.csproj AM.TestTask.Data.Relational.EntityFramework/
COPY src/backend/AM.TestTask.Data.Relational.Sync.Abstractions/AM.TestTask.Data.Relational.Sync.Abstractions.csproj AM.TestTask.Data.Relational.Sync.Abstractions/
COPY src/backend/AM.TestTask.Data.Relational.Sync.PostgreSql/AM.TestTask.Data.Relational.Sync.PostgreSql.csproj AM.TestTask.Data.Relational.Sync.PostgreSql/
COPY src/backend/AM.TestTask.Data.Cache/AM.TestTask.Data.Cache.csproj AM.TestTask.Data.Cache/

RUN dotnet restore AM.TestTask.Api/AM.TestTask.Api.csproj

# Копируем исходный код один раз для всех
COPY src/backend/. .

FROM prepare-restore AS build-api
WORKDIR /src/AM.TestTask.Api
RUN dotnet publish AM.TestTask.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ для API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api-final
WORKDIR /app
COPY --from=build-api /app/publish .
ENTRYPOINT ["dotnet", "AM.TestTask.Api.dll"]



FROM prepare-restore AS build-migrator
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Билдим проект (для ef)
RUN dotnet build AM.TestTask.Api/AM.TestTask.Api.csproj -c Release

RUN dotnet ef migrations bundle \
    --project AM.TestTask.Data.Relational.AppDatabase.PostgreSql/AM.TestTask.Data.Relational.AppDatabase.PostgreSql.csproj \
    --startup-project AM.TestTask.Api/AM.TestTask.Api.csproj \
    --context ApplicationDatabaseContext \
    --configuration Release \
    --no-build \
    -o /app/efbundle \
    --force

# Финальный образ для Мигратора
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrator-final
WORKDIR /app
COPY --from=build-migrator /app/efbundle .
RUN chmod +x ./efbundle

ENTRYPOINT ["./efbundle"]