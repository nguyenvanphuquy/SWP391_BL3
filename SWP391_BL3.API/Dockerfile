FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["SWP391_BL3.API/SWP391_BL3.API.csproj", "SWP391_BL3.API/"]
COPY ["SWP391_BL3.BLL/SWP391_BL3.BLL.csproj", "SWP391_BL3.BLL/"]
COPY ["SWP391_BL3.DAL/SWP391_BL3.DAL.csproj", "SWP391_BL3.DAL/"]

RUN dotnet restore "SWP391_BL3.API/SWP391_BL3.API.csproj"

COPY . .

WORKDIR "/src/SWP391_BL3.API"

RUN dotnet build "SWP391_BL3.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "SWP391_BL3.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SWP391_BL3.API.dll"]