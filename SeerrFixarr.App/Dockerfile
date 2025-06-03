FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SeerrFixarr.App/SeerrFixarr.App.csproj", "SeerrFixarr.App/"]
RUN dotnet restore "SeerrFixarr.App/SeerrFixarr.App.csproj"
COPY . .
WORKDIR "/src/SeerrFixarr.App"
RUN dotnet build "SeerrFixarr.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SeerrFixarr.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=true \
    /p:DebugType=none \
    /p:DebugSymbols=false \
    /p:Optimize=true \
    /p:RuntimeIdentifier=linux-x64 \
    /p:PublishSingleFile=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0-preview-noble-chiseled-composite-extra
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT="80"
ENV CULTURE="de-CH"
ENV SERILOG__MINIMUMLEVEL__Default="Information"

ENTRYPOINT ["./SeerrFixarr.App"]