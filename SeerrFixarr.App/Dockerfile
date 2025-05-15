FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SeerrFixarr.App/SeerrFixarr.App.csproj", "SeerrFixarr.App/"]
RUN dotnet restore "SeerrFixarr.App/SeerrFixarr.App.csproj"
COPY . .
WORKDIR "/src/SeerrFixarr.App"
RUN dotnet build "SeerrFixarr.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SeerrFixarr.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeerrFixarr.App.dll"]
