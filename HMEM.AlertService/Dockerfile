FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./HMEM.AlertService/HMEM.AlertService.csproj ./HMEM.AlertService/
COPY ./HMEM.Common/HMEM.Common.csproj ./HMEM.Common/
COPY ./HMEM.Data/HMEM.Data.csproj ./HMEM.Data/
COPY ./HMEM.MessageBroker/HMEM.MessageBroker.csproj ./HMEM.MessageBroker/

RUN dotnet restore ./HMEM.AlertService/HMEM.AlertService.csproj

COPY . .

WORKDIR /src/HMEM.AlertService
RUN dotnet publish -c Release -o /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out ./
COPY ./HMEM.AlertService/appsettings.json ./

ENTRYPOINT ["dotnet", "HMEM.AlertService.dll"]
