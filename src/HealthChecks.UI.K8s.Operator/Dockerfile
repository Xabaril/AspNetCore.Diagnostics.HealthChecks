#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj", "src/HealthChecks.UI.K8s.Operator/"]
RUN dotnet restore "src/HealthChecks.UI.K8s.Operator/HealthChecks.UI.K8s.Operator.csproj"
COPY . .
WORKDIR "/src/src/HealthChecks.UI.K8s.Operator"
RUN dotnet build "HealthChecks.UI.K8s.Operator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthChecks.UI.K8s.Operator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthChecks.UI.K8s.Operator.dll"]