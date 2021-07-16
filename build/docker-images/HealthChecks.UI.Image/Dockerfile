FROM mcr.microsoft.com/dotnet/aspnet:5.0.7-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0.301-buster-slim AS sdk-with-node

ENV NODE_VERSION 14.17.0
ENV NODE_DOWNLOAD_SHA 3d06eabc73ec8626337bff370474306eac1c3c21122f677720d154c556ceafaf
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs

FROM sdk-with-node AS updated-npm
RUN npm i -g npm

FROM updated-npm AS build
WORKDIR /src
COPY AspNetCore.Diagnostics.HealthChecks.sln ./
COPY global.json ./
COPY Directory.Build.props ./
COPY ./build/dependencies.props ./build/
COPY ./src/HealthChecks.UI/build ./src/HealthChecks.UI/build
COPY ./src/HealthChecks.UI/assets ./src/HealthChecks.UI/assets
COPY ./src/HealthChecks.UI/client ./src/HealthChecks.UI/client
COPY ./build/docker-images/HealthChecks.UI.Image/HealthChecks.UI.Image.csproj ./build/docker-images/HealthChecks.UI.Image/
COPY . .
WORKDIR /src/build/docker-images/HealthChecks.UI.Image
RUN rm ../../../global.json
RUN dotnet restore -nowarn:msb3202,nu1503
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HealthChecks.UI.Image.dll"]
