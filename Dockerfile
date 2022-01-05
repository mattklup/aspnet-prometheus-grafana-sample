# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /sample

COPY ./src ./src/
COPY Sample.sln .

RUN dotnet restore

RUN dotnet publish -c Release -o build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /sample
COPY --from=build-env /sample/build .
ENTRYPOINT ["dotnet"]
CMD ["AspNetCore.WebApi.dll"]
