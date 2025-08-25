FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SalesApp.sln ./
COPY src/Domain/ ./src/Domain/
COPY src/Application/ ./src/Application/
COPY src/Infrastructure/ ./src/Infrastructure/
COPY src/WebApi/ ./src/WebApi/

RUN dotnet restore SalesApp.sln

RUN dotnet publish src/WebApi/SalesApp.WebApi.csproj -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "SalesApp.WebApi.dll"]
