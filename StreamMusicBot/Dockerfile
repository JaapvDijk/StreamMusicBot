# FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# WORKDIR /app 
# EXPOSE 80

# COPY /bin/Debug .
# ENTRYPOINT ["FrameworkConsoleApp.exe"]

# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

RUN dotnet build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "StreamMusicBot.dll"]