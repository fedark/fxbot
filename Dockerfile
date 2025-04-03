# syntax=docker/dockerfile:1-labs

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY --parents=true */*.csproj .
RUN dotnet restore FxBot/FxBot.csproj

COPY . .
RUN dotnet build FxBot/FxBot.csproj -c ${BUILD_CONFIGURATION} -o /src/build --no-restore


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish FxBot/FxBot.csproj -c ${BUILD_CONFIGURATION} -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG PYTHON_ENV=py_env
WORKDIR /app
COPY --from=publish /app/publish .

# configure python
RUN apt-get update -y
RUN apt-get install -y python3
RUN apt-get install -y python3-pip
RUN apt-get install -y python3-venv
RUN python3 -m venv ${PYTHON_ENV}
RUN ${PYTHON_ENV}/bin/pip install matplotlib

ENTRYPOINT ["dotnet", "FxBot.dll"]