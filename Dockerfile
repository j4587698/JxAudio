FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
VOLUME /app/config
VOLUME /app/log

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN apt-get update && \
    apt-get install -y zip
WORKDIR /src/Plugins
COPY ["Plugins/LocalFileProviderPlugin/LocalFileProviderPlugin.csproj", "LocalFileProviderPlugin/"]
RUN dotnet restore "LocalFileProviderPlugin/LocalFileProviderPlugin.csproj"
COPY --link . .
WORKDIR "/src/Plugins/LocalFileProviderPlugin"
RUN dotnet build "LocalFileProviderPlugin.csproj" -c $BUILD_CONFIGURATION -o /app/LocalFileProviderPlugin
RUN mkdir /app/LocalFileProviderPlugin/LocalFileProviderPlugin &&  \
    cp /app/LocalFileProviderPlugin/*.dll /app/LocalFileProviderPlugin/LocalFileProviderPlugin && \
    cd /app/LocalFileProviderPlugin && \
    zip -r LocalFileProviderPlugin.jxaudio ./LocalFileProviderPlugin
WORKDIR /src/Plugins
COPY ["Plugins/AListProviderPlugin/AListProviderPlugin.csproj", "AListProviderPlugin/"]
RUN dotnet restore "AListProviderPlugin/AListProviderPlugin.csproj"
COPY . .
WORKDIR "/src/Plugins/AListProviderPlugin"
RUN dotnet build "AListProviderPlugin.csproj" -c $BUILD_CONFIGURATION -o /app/AListProviderPlugin
RUN mkdir /app/AListProviderPlugin/AListProviderPlugin &&  \
    cp /app/AListProviderPlugin/*.dll /app/AListProviderPlugin/AListProviderPlugin && \
    cd /app/AListProviderPlugin && \
    zip -r AListProviderPlugin.jxaudio ./AListProviderPlugin
WORKDIR /src
COPY ["JxAudio.Web/JxAudio.Web.csproj", "JxAudio.Web/"]
RUN dotnet restore "JxAudio.Web/JxAudio.Web.csproj"
COPY --link . .
WORKDIR "/src/JxAudio.Web"
RUN dotnet restore -a $TARGETARCH

RUN dotnet publish -a $TARGETARCH --no-restore -o /app/publish
RUN mkdir /app/publish/plugins && \
    cp /app/LocalFileProviderPlugin/LocalFileProviderPlugin.jxaudio /app/publish/plugins && \
    cp /app/AListProviderPlugin/AListProviderPlugin.jxaudio /app/publish/plugins

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN mkdir /app/config && mkdir /app/log && chown -R $APP_UID:$APP_UID /app/config /app/log
USER $APP_UID
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
VOLUME /app/config
VOLUME /app/log
COPY --link --from=build /app/publish .
ENTRYPOINT ["./JxAudio.Web"]
