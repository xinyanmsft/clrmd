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

RUN dotnet build --no-restore -c $BUILD_CONFIGURATION
RUN dotnet publish -o /app-mempeek -r linux-x64
RUN ln -s /app-mempeek/mempeek /usr/bin/mempeek
WORKDIR /tools

RUN echo "trap : TERM INT; sleep infinity & wait" > /entrypoint.sh

ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]