# syntax=docker/dockerfile:1
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
WORKDIR /src/api
COPY . .
COPY --from=shared-dtos . /src/shared-dtos/
RUN dotnet publish ArzenalStoreApi/Arzenal.Store.Api.csproj -c Release -a $TARGETARCH --self-contained false -o /out -p:SharedDtosProjectPath=/src/shared-dtos/Arzenal.Dto.csproj
RUN mkdir -p /artifact-storage

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build --chown=$APP_UID:0 /out .
COPY --from=build --chown=$APP_UID:0 /artifact-storage /var/lib/arzenal/apps
ENV ASPNETCORE_URLS=http://+:8080
ENV Storage__AppFilesPath=/var/lib/arzenal/apps
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Arzenal.Store.Api.dll"]
