FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_DOCKIT_TOKEN
ENV ASPNETCORE_ENVIRONMENT=Development
WORKDIR /src
COPY ["./generationapi/generationapi.csproj", "./generationapi/"]
COPY ["./common/common.csproj", "./common/"]
# COPY ["./docProcessor/docProcessor.csproj", "./docProcessor/"]
COPY ["./generationapi/nuget.config", "./"]
RUN GITHUB_DOCKIT_TOKEN=$GITHUB_DOCKIT_TOKEN dotnet restore "./generationapi/generationapi.csproj" --configfile nuget.config
COPY . .
WORKDIR "/src/"
RUN dotnet build "./generationapi/generationapi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./generationapi/generationapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "generationapi.dll", "--environment=Development"]
