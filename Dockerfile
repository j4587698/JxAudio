FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN apt-get update && \
    apt-get install -y zip 
WORKDIR /src
COPY --link . .
WORKDIR /src/Plugins
COPY ["Plugins/LocalFileProviderPlugin/LocalFileProviderPlugin.csproj", "LocalFileProviderPlugin/"]
RUN dotnet restore "LocalFileProviderPlugin/LocalFileProviderPlugin.csproj"
WORKDIR "/src/Plugins/LocalFileProviderPlugin"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/LocalFileProviderPlugin
RUN mkdir /app/LocalFileProviderPlugin/LocalFileProviderPlugin &&  \
    cd /app/LocalFileProviderPlugin && \
    find . -maxdepth 1 -name "*.dll" ! -name "JxAudio*" -exec cp {} /app/LocalFileProviderPlugin/LocalFileProviderPlugin \; && \
    zip -r LocalFileProviderPlugin.jxaudio ./LocalFileProviderPlugin
WORKDIR /src/Plugins
COPY ["Plugins/AListProviderPlugin/AListProviderPlugin.csproj", "AListProviderPlugin/"]
RUN dotnet restore "AListProviderPlugin/AListProviderPlugin.csproj"
WORKDIR "/src/Plugins/AListProviderPlugin"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/AListProviderPlugin
RUN mkdir /app/AListProviderPlugin/AListProviderPlugin &&  \
    cd /app/AListProviderPlugin && \
    find . -maxdepth 1 -name "*.dll" ! -name "JxAudio*" -exec cp {} /app/AListProviderPlugin/AListProviderPlugin \; && \
    zip -r AListProviderPlugin.jxaudio ./AListProviderPlugin
WORKDIR /src
COPY ["JxAudio.Web/JxAudio.Web.csproj", "JxAudio.Web/"]
RUN dotnet restore "JxAudio.Web/JxAudio.Web.csproj"
WORKDIR "/src/JxAudio.Web"
RUN dotnet restore -a $TARGETARCH

RUN dotnet publish -a $TARGETARCH --no-restore -o /app/publish
RUN mkdir /app/publish/plugins && \
    cp /app/LocalFileProviderPlugin/LocalFileProviderPlugin.jxaudio /app/publish/plugins && \
    cp /app/AListProviderPlugin/AListProviderPlugin.jxaudio /app/publish/plugins

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 4587
VOLUME /app/config
VOLUME /app/log
COPY --link --from=build /app/publish .
ENTRYPOINT ["./JxAudio.Web"]
