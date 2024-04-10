FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AW_Daemon/AW_Daemon.csproj", "AW_Daemon/"]
RUN dotnet restore "AW_Daemon/AW_Daemon.csproj"
COPY . .
WORKDIR "/src/AW_Daemon"
RUN dotnet build "AW_Daemon.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AW_Daemon.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AW_Daemon.dll"]
