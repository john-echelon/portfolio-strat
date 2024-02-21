# portfolio-strat
>
> Generates a report of investment outcomes based on varying distributions of principal, contributions, and lump sum investments.

[![.NET][dotnet-image]][dotnet-url]

The goal of these reports is to provide insight in determining an ideal or optimal portfolio allocation based on the one's performance criteria, and risk tolerance.

Allocations consist of varying distributions of upfront principal, contributions, accounting for time in the market and consideration of lump sum investments in the event of a market trough or recessionary event.

## Installation

```sh
git clone https://github.com/john-echelon/portfolio-strat.git
```

## Setup

The following examples demonstrate how to run a program in either debug or release mode.

```sh
cd portfolio-strat
dotnet run --project=PortfolioStrat.Console
```

Use the -- delimeter to allow all arguments after this delimeter to be passed onto the application to run.
> -c|--configuration {Debug|Release}

```sh
cd portfolio-strat/PortfolioStrat.Console
dotnet run -c release -- --help
```

Publish as exe/binary:

```sh
cd portfolio-strat
dotnet publish
```

## Usage examples

An investment fund allocation report, 9% annual rate of return, 10k total fund
allocation over the span of 10 years (default):

```sh
foliostrat -a 10000 -t 10 -r 0.09 --drop-pct 0.47 --time-to-recover 0.50
```

An investment fund allocation report with 15% annual rate of return with no
recessionary event:

```sh
foliostrat -r 0.15 --include-recession=false
```

An investment fund allocation report with 15% annual rate of return with a
yearly breakdown for allocation strategy #0:

```sh
foliostrat -r 0.15 -b 0
```

An investment fund with a starting total allocation of 100k that spans 33 years
with a max time to recover of 13.5 years after a market recessionary event:

```sh
foliostrat -a 100000 -t 33 --time-to-recover 13.5
```

## Release History

* 0.1.0
  * Support non-intergral timespans
  * Added README
  * Clean up & prettify console output
  * Reorganized as a multi-project solution
  * CLI support
    * Support for criteria selection

## TODO

* build data model for yearly breakdowns for ease of importing data for reporting
  * Show other stats e.g.avg, median, mode, etc
  * Show yearly breakdown as Total Deposits, Total Contributions, Interest, FV (Balance)
* Unit Tests
* Add background information, formulas to README

## Contributing

1. Fork it (<https://github.com/yourname/yourproject/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

<!-- Markdown link & img dfn's -->
[dotnet-image]: https://img.shields.io/badge/--512BD4?logo=.net&logoColor=ffffff
[dotnet-url]: https://dotnet.microsoft.com/
