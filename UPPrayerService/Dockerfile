#
# TEMPORARY COMPILER CONTAINER
#
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /upd

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o bin

#
# RUNTIME IMAGE
#
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
EXPOSE 443/tcp
EXPOSE 80/tcp
WORKDIR /upd
VOLUME ["./db/"]

# Copy in binaries from build image
COPY --from=build-env /upd/bin ./bin

# Copy in certificate
ADD ./cert-production.pfx ./cert/cert.pfx

# Start the service on container
WORKDIR ./bin
ENTRYPOINT ["dotnet", "UPPrayerService.dll"]