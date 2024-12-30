FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Optional: install curl to check connectivity
RUN apt-get update && apt-get install -y curl
RUN curl -I https://api.nuget.org/v3/index.json

RUN dotnet tool install --global dotnet-ef --version 9.0.0
ENV PATH="$PATH:/root/.dotnet/tools"

COPY backend/WebAPI.csproj ./WebApi/
COPY backend/appsettings.json ./WebApi/
COPY backend/appsettings.Development.json ./WebApi/

WORKDIR /app/WebApi

# Test adding and removing a small package
RUN dotnet add package Microsoft.Extensions.Logging.Console
RUN dotnet remove package Microsoft.Extensions.Logging.Console

COPY backend/ .
RUN dotnet build

CMD ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
