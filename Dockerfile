# STAGE 1 - Builder
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

#* Define project name for shortcuts
ARG PROJECT=MarketPriceAPI
WORKDIR /src

#* Copy project files and restore depencies
COPY src/${PROJECT}/${PROJECT}.csproj src/${PROJECT}/
COPY ${PROJECT}.sln .

WORKDIR /src/src/${PROJECT}
RUN dotnet restore

#* Copy source code and publish
COPY src/${PROJECT}/ ./
RUN dotnet publish -c Release -o /app/publish

# STAGE 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS run
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarketPriceAPI.dll"]