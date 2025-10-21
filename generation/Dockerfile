FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV ASPNETCORE_ENVIRONMENT Development
WORKDIR /src
COPY ["./generationapi/generationapi.csproj", "./generationapi/"]
COPY ["./docProcessor/docProcessor.csproj", "./docProcessor/"]
RUN dotnet restore "./generationapi/generationapi.csproj"
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
