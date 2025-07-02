FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Apps/ConsoleApp/ConsoleApp.csproj", "Apps/ConsoleApp/"]
COPY ["Core/DocumentationGenerator/DocumentationGenerator.csproj", "Core/DocumentationGenerator/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "Apps/ConsoleApp/ConsoleApp.csproj"
COPY . .
WORKDIR "/src/Apps/ConsoleApp"
RUN dotnet build "./ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ConsoleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]
