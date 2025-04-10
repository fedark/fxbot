# About

This is the telegram bot to utilize the FX rate on a given date, specifically, the USD/BYN rate.  
Possibilities:
- get the rate on a given date or on the day of current month
- convert price in BYN to price in USD on a given date or on the day of current month
- show USD/BYN rate history over a given period

The corresponding commands are available in telegram chat:
- ﻿`{date}` or `{day}` - FX rate value on any date or any day of current month in the past
- `{value} {date}` or `{value} {day}` - Convert value to foreign currency on any date or any day of current month in the past. Multiple lines can be provided
- `/history` - FX rate dynamics plot for a date range

# Technical details

There are 3 separate projects:
- `QuoteService` to communicate with external bank API using `HttpClient`. It returns either a numeric response or the stream containing the rate dynamics image. The underlying program to plot the images is `python`
- `BotInfrastructure` to communicate with telegram API. It contains the bot client implementation as well as supported bot commands. The commands handling is implemented using the Chain of Responsibility pattern: each command tries to process a request, if it's not possible the flow goes to the next command in the chain
- `FxBot` contains an entry point of the program as well as the settings/services configuration using `HostBuilder`

All projects are structured in the way that abstractions and implementations are separated, so that it can be easily splitted in future to separate projects making the project architechture even more clean.

Also, there is docker support. The `docker-compose` configuration includes:
- `fxbot` service for the program
- `redis` service for caching (currently, the rate dynamics images are being cached since it takes quite a time to get response from API over a long period and plotting an image using external program
- `Bot__Token` environment variable required to build the image, it should be set within a terminal
