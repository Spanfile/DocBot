# DocBot

A bot for [Discord](https://discordapp.com) that fetches and provides articles from a variety of different programming documentation sources. [Invite it to your guild here!](https://discordapp.com/api/oauth2/authorize?client_id=393834622850564106&permissions=3072&scope=bot)

## Basic usage

To query a given site, use the `;doc <documentation> <query>` command. For more help on commands and documentation sites, use `;doc help` and `;doc docs`.

## Supported documentation sources

* [Arch Wiki](https://wiki.archlinux.org/)
* [C++ reference](http://en.cppreference.com/w/)
* [Java SE 9 & JDK 9](https://docs.oracle.com/javase/9/docs/api/overview-summary.html)
* [.NET API Browser](https://docs.microsoft.com/en-us/dotnet/api/)
* [Python 3](https://docs.python.org/3/) and [2.7](https://docs.python.org/2.7/)
* [Rust](https://doc.rust-lang.org)

## Running it yourself

### Third-party requirements

DocBot requires PhantomJS for certain documentation sources. Download the binary and place it somewhere accessible. On Linux, there is a hidden requirement for the package `fontconfig`.

### Sample config

Next to the DocBot library, create a file called `config.json` and fill it with the appropriate values.

```json
{
  "prefix": ";doc ",
  "discord": {
    "token": "<discord bot user token here>"
  },
  "perf": {
    "collectionInterval": 5000,
    "samples": 60
  },
  "useragent": "<user agent - use a descriptive one>",
  "phantomjsPath": "<path to the PhantomJS executable>"
}
```

### Diagnostics

DocBot periodically collects diagnostic info about itself. These include

* Memory use
* Latency to Discord API gateway
* Cache size
* .NET GC information

To view this information, use `;doc diag`.
