using System;
using Microsoft.Extensions.Logging;

namespace Azure.Functions.Cli.Diagnostics
{
    public class ColoredConsoleLoggerProvider : ILoggerProvider
    {
        private readonly LoggingFilterHelper _loggingFilterOptions;

        public ColoredConsoleLoggerProvider(LoggingFilterHelper loggingFilterOptions)
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
