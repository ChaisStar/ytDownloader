# === СТАДІЯ 1: Збірка React (Vite) ===
FROM node:18 AS build-frontend
WORKDIR /app
COPY ./ytdownloader-client/package.json ./ytdownloader-client/yarn.lock ./
RUN yarn install --frozen-lockfile
COPY ./ytdownloader-client ./
RUN yarn build

# === СТАДІЯ 2: Збірка .NET 9.0 API ===
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-backend
WORKDIR /src

# Оптимізоване кешування залежностей
COPY *.sln ./
COPY docker-compose.dcproj ./
COPY YtDownloader.Api/*.csproj YtDownloader.Api/
COPY YtDownloader.Base/*.csproj YtDownloader.Base/
COPY YtDownloader.Core/*.csproj YtDownloader.Core/
COPY YtDownloader.Database/*.csproj YtDownloader.Database/
COPY ytdownloader-client/*.esproj ytdownloader-client/
RUN dotnet restore YtDownloader.sln

# Копіюємо всі вихідні файли та збираємо
COPY . .
RUN dotnet publish YtDownloader.Api/YtDownloader.Api.csproj -c Release -o /app/publish

# === СТАДІЯ 3: Запуск контейнера ===
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Оновлення системи та встановлення Python3, pip, ffmpeg, yt-dlp
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl ca-certificates xz-utils python3 python3-pip pipx nodejs npm && \
    update-ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Додаємо pipx у PATH перед встановленням yt-dlp
ENV PATH="/root/.local/bin:$PATH"

# Встановлення yt-dlp з залежностями за замовчуванням (включає yt-dlp-ejs для розв'язання JavaScript-викликів YouTube)
# Створюємо конфігураційний файл yt-dlp для включення JavaScript runtime та remote components
RUN pipx install "yt-dlp[default]" && \
    mkdir -p /root/.config/yt-dlp && \
    (echo "js-runtimes node" && \
     echo "remote-components ejs:npm") > /root/.config/yt-dlp/config.txt

# Встановлення найновішого ffmpeg
RUN curl -L https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz | tar -xJ && \
    mv ffmpeg-*-static/ffmpeg /usr/local/bin/ && \
    mv ffmpeg-*-static/ffprobe /usr/local/bin/ && \
    chmod a+rx /usr/local/bin/ffmpeg /usr/local/bin/ffprobe

# Копіюємо .NET API
COPY --from=build-backend /app/publish .

# Копіюємо React у static
COPY --from=build-frontend /app/dist ./static

# Створюємо томи для збереження відео
VOLUME ["/tmp/youtube", "/tmp/youtube_later", "/tmp/finished"]

# Відкриваємо порт
EXPOSE 8085

# Запускаємо API
CMD ["dotnet", "YtDownloader.Api.dll"]
