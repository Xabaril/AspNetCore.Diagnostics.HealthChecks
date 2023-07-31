FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS sdk-with-node

ENV NODE_VERSION 18.16.0
ENV NODE_DOWNLOAD_SHA fc83046a93d2189d919005a348db3b2372b598a145d84eb9781a3a4b0f032e95
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

FROM sdk-with-node AS updated-npm
RUN npm i -g npm@9

FROM updated-npm AS build
WORKDIR /src
COPY AspNetCore.Diagnostics.HealthChecks.sln ./
COPY Directory.Build.props ./
COPY Directory.Build.targets ./
COPY ./build/versions.props ./build/
COPY ./src/HealthChecks.UI/build ./src/HealthChecks.UI/build
COPY ./src/HealthChecks.UI/assets ./src/HealthChecks.UI/assets
COPY ./src/HealthChecks.UI/client ./src/HealthChecks.UI/client
COPY ./build/docker-images/HealthChecks.UI.Image/HealthChecks.UI.Image.csproj ./build/docker-images/HealthChecks.UI.Image/
COPY . .
WORKDIR /src/build/docker-images/HealthChecks.UI.Image
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HealthChecks.UI.Image.dll"]
