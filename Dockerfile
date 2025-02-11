# === СТАДІЯ 1: Збірка React (Vite) ===
FROM node:18 AS build-frontend
WORKDIR /app
COPY ytdownloader-client/package.json ytdownloader-client/package-lock.json ./
RUN npm install
COPY ytdownloader-client ./
RUN npm run build

# === СТАДІЯ 2: Збірка .NET 9.0 API ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-backend
WORKDIR /src
COPY YtDownloader.Api/*.csproj YtDownloader.Api/
RUN dotnet restore YtDownloader.Api/YtDownloader.Api.csproj
COPY YtDownloader.Api/. YtDownloader.Api/
RUN dotnet publish YtDownloader.Api/YtDownloader.Api.csproj -c Release -o /app/publish

# === СТАДІЯ 3: Запуск контейнера ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

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

    ENV PATH="/root/.local/bin:$PATH"

# Копіюємо .NET API
COPY --from=build-backend /app/publish .

# Копіюємо React у static
COPY --from=build-frontend /app/dist ./static

VOLUME ["/tmp/youtube", "/tmp/youtube_later", "/tmp/finished"]

# Відкриваємо порт
EXPOSE 8085

# Запускаємо API
CMD ["dotnet", "YtDownloader.Api.dll"]
