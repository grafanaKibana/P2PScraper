FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER root
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update; apt-get clean
RUN apt-get install -y wget
RUN apt-get install -y curl
RUN apt-get install -y gnupg
RUN apt-get update && apt-get -y install libglib2.0-0 libxi6 libnss3
RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list
RUN apt-get update && apt-get -y install google-chrome-stable

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["P2PScraper/P2PScraper.csproj", "P2PScraper/"]
RUN dotnet restore "P2PScraper/P2PScraper.csproj"
COPY . .
WORKDIR "/src/P2PScraper"
RUN dotnet build "P2PScraper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "P2PScraper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true -p:PublishChromeDriver=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "P2PScraper.dll"]