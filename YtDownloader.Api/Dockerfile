# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# Оновлення системи та встановлення Python3, pip, ffmpeg, yt-dlp
RUN apt-get update && \
    apt-get install -y curl ca-certificates xz-utils python3 python3-pip pipx && \
    update-ca-certificates

# Використання pipx для встановлення yt-dlp
RUN pipx install yt-dlp

# Встановлення найновішого ffmpeg
RUN curl -L https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz | tar -xJ && \
    mv ffmpeg-*-static/ffmpeg /usr/local/bin/ && \
    mv ffmpeg-*-static/ffprobe /usr/local/bin/ && \
    chmod a+rx /usr/local/bin/ffmpeg /usr/local/bin/ffprobe

#USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV PATH="/root/.local/bin:$PATH"

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["YtDownloader.Api/YtDownloader.Api.csproj", "YtDownloader.Api/"]
RUN dotnet restore "./YtDownloader.Api/YtDownloader.Api.csproj"
COPY . .
WORKDIR "/src/YtDownloader.Api"
RUN dotnet build "./YtDownloader.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YtDownloader.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YtDownloader.Api.dll"]