using System;
using Microsoft.Extensions.Logging;

namespace Azure.Functions.Cli.Diagnostics
{
    public class ColoredConsoleLoggerProvider : ILoggerProvider
    {
        private readonly LoggingFilterOptions _loggingFilterOptions;

        public ColoredConsoleLoggerProvider(LoggingFilterOptions loggingFilterOptions)
        {
            _loggingFilterOptions = loggingFilterOptions;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ColoredConsoleLogger(categoryName, _loggingFilterOptions);
        }

        public void Dispose()
        {
        }
    }
}
