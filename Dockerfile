FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LiveNow.CRM.Core/LiveNow.CRM.Core.csproj", "LiveNow.CRM.Core/"]
COPY ["LiveNow.CRM.Infrastructure/LiveNow.CRM.Infrastructure.csproj", "LiveNow.CRM.Infrastructure/"]
COPY ["LiveNow.CRM.Infrastructure.PostgreSql/LiveNow.CRM.Infrastructure.PostgreSql.csproj", "LiveNow.CRM.Infrastructure.PostgreSql/"]
COPY ["LiveNow.CRM.API/LiveNow.CRM.API.csproj", "LiveNow.CRM.API/"]
RUN dotnet restore "LiveNow.CRM.API/LiveNow.CRM.API.csproj"
COPY . .
RUN dotnet publish "LiveNow.CRM.API/LiveNow.CRM.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
COPY --from=build /app/publish .
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet LiveNow.CRM.API.dll --urls http://0.0.0.0:${PORT:-10000}"]
