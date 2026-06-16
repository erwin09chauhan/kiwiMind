FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY KiwiMind.slnx .
COPY src/KiwiMind.Domain/KiwiMind.Domain.csproj src/KiwiMind.Domain/
COPY src/KiwiMind.Application/KiwiMind.Application.csproj src/KiwiMind.Application/
COPY src/KiwiMind.Infrastructure/KiwiMind.Infrastructure.csproj src/KiwiMind.Infrastructure/
COPY src/KiwiMind.Api/KiwiMind.Api.csproj src/KiwiMind.Api/
RUN dotnet restore src/KiwiMind.Api/KiwiMind.Api.csproj

COPY src/ src/
RUN dotnet publish src/KiwiMind.Api/KiwiMind.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
USER app

COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "KiwiMind.Api.dll"]
