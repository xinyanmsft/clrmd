FROM mcr.microsoft.com/dotnet/core/sdk:2.2
ENV ASPNETCORE_ENVIRONMENT=Development
ENV BUILD_CONFIGURATION=Debug
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
EXPOSE 5000

# install ClrMD dependency libdl.so
RUN apt-get update
RUN apt-get install -y libc6-dev

WORKDIR /src
COPY ["Samples/LinuxLiveTest/mempeek.csproj", "./Samples/LinuxLiveTest/"]
COPY ["Microsoft.Diagnostics.Runtime/Microsoft.Diagnostics.Runtime.csproj", "./Microsoft.Diagnostics.Runtime/"]

WORKDIR /src/Samples/LinuxLiveTest
RUN dotnet restore

COPY ["Samples/LinuxLiveTest/.", "/src/Samples/LinuxLiveTest/"]
COPY ["Microsoft.Diagnostics.Runtime/.", "/src/Microsoft.Diagnostics.Runtime/"]

RUN dotnet publish -c $BUILD_CONFIGURATION -o /app-mempeek

# Second stage build
FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9

# Upgrading apk and adding basic tools
RUN apk update \
    && apk upgrade \
    && apk add --update --no-cache bash \
    && apk add --update curl \
    && apk add --update --no-cache openssl

COPY --from=0 /app-mempeek /app-mempeek

RUN echo -e '#!/bin/bash\ndotnet /app-mempeek/mempeek.dll $@' > /app-mempeek/mempeek.sh
RUN chmod +x /app-mempeek/mempeek.sh
RUN ln -s /app-mempeek/mempeek.sh /usr/bin/mempeek
WORKDIR /app-mempeek

RUN echo "trap : TERM INT; while true; do slee 10; done & wait" > /entrypoint.sh
ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]
