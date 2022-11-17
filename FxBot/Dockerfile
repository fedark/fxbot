# build app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /fxbot
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o publish


# build app container

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /fxbot
COPY --from=build /fxbot/publish .

# configure python
RUN apt-get update -y
RUN apt-get install -y python3
RUN apt-get install -y python3-pip
RUN python3 -m pip install --upgrade pip wheel setuptools
RUN python3 -m pip install matplotlib

ENTRYPOINT ["dotnet", "FxBot.dll"]