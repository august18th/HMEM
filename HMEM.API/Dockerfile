FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./HMEM.API/HMEM.API.csproj ./HMEM.API/
COPY ../HMEM.Common/HMEM.Common.csproj ./HMEM.Common/
COPY ../HMEM.Data/HMEM.Data.csproj ./HMEM.Data/
COPY ./HMEM.MessageBroker/HMEM.MessageBroker.csproj ./HMEM.MessageBroker/

RUN dotnet restore ./HMEM.API/HMEM.API.csproj

COPY . .

WORKDIR /src/HMEM.API
RUN dotnet publish -c Release -o /app/out

FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "HMEM.API.dll"]
