# build app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS sdk
WORKDIR /fxbot

COPY ./FxBot/*.csproj ./FxBot/
COPY ./QuoteService/*.csproj ./QuoteService/
RUN dotnet restore ./FxBot/FxBot.csproj

COPY ./FxBot/. ./FxBot/
COPY ./QuoteService/. ./QuoteService/
WORKDIR /fxbot/FxBot
RUN dotnet publish -c Release -o publish


# build app container

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
WORKDIR /fxbot
COPY --from=sdk /fxbot/FxBot/publish ./


# configure python

RUN apt-get update -y
RUN apt-get install -y python3
RUN apt-get install -y python3-pip
RUN python3 -m pip install --upgrade pip wheel setuptools
RUN python3 -m pip install matplotlib

ENTRYPOINT ["dotnet", "FxBot.dll"]