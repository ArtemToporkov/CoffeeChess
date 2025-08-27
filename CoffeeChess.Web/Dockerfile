FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeeChess.Web/CoffeeChess.Web.csproj", "CoffeeChess.Web/"]
COPY ["CoffeeChess.Domain/CoffeeChess.Domain.csproj", "CoffeeChess.Domain/"]
COPY ["CoffeeChess.Infrastructure/CoffeeChess.Infrastructure.csproj", "CoffeeChess.Infrastructure/"]
COPY ["CoffeeChess.Application/CoffeeChess.Application.csproj", "CoffeeChess.Application/"]
RUN dotnet restore "CoffeeChess.Web/CoffeeChess.Web.csproj"
COPY . .
WORKDIR "/src/CoffeeChess.Web"
RUN dotnet build "./CoffeeChess.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeeChess.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeChess.Web.dll"]