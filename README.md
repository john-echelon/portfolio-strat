# portfolio-strat
> Generates a report of investment outcomes based on varying distributions of principal, contributions, and lump sum investments.

[![.NET][dotnet-image]][dotnet-url]]

The goal of these reports is to provide insight in determining an ideal or optimal portfolio allocation based on the one's performance criteria, and risk tolerance.

Allocations consist of varying distributions of upfront principal, contributions, accounting for time in the market and consideration of lump sum investments in the event of a market trough or recessionary event.

## Installation

```sh
git clone https://github.com/john-echelon/portfolio-strat.git
```

## Usage example

A few motivating and useful examples of how your product can be used. Spice this up with code blocks and potentially more screenshots.

_For more examples and usage, please refer to the [Wiki][wiki]._

## Development setup

Describe how to install all development dependencies and how to run an automated test-suite of some kind. Potentially do this for multiple platforms.

```sh
dotnet run
```

## Release History

* 0.0.0
  * Support non-intergral timespans
  * Add README
  * Clean up & prettify console output

## TODO

* build data model for yearly breakdowns for ease of importing data for reporting
  * Show other stats e.g.avg, median, mode, etc
* CLI support
  * Support for criteria selection
* Unit Tests
* Add additional background information, e.g. formulas to README

## Contributing

1. Fork it (<https://github.com/yourname/yourproject/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request

<!-- Markdown link & img dfn's -->
[dotnet-image]: https://img.shields.io/badge/--512BD4?logo=.net&logoColor=ffffff
[dotnet-url]: https://dotnet.microsoft.com/
[wiki]: https://github.com/yourname/yourproject/wiki

