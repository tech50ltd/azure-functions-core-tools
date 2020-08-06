using Azure.Functions.Cli.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Azure.Functions.Cli.Tests
{
    public class ColoredConsoleLoggerTests
    {
        [Theory]
        [InlineData("somelog", false)]
        [InlineData("Worker process started and initialized.", true)]
        [InlineData("Worker PROCESS started and initialized.", true)]
        [InlineData("Worker process started.", false)]
        [InlineData("Host lock lease acquired by instance ID", true)]
        [InlineData("Host lock lease acquired by instance id", true)]
        [InlineData("Host lock lease", false)]
        public void DoesMessageStartsWithWhiteListedPrefix_Tests(string formattedMessage, bool expected)
        {
            ColoredConsoleLogger coloredConsoleLogger = new ColoredConsoleLogger("test", new LoggingFilterOptions(true));
            Assert.Equal(expected, coloredConsoleLogger.DoesMessageStartsWithWhiteListedPrefix(formattedMessage));
        }

        [Theory]
        [InlineData("somelog", false)]
        [InlineData("Worker process started and initialized.", true)]
        [InlineData("Worker PROCESS started and initialized.", true)]
        [InlineData("Worker process started.", false)]
        [InlineData("Host lock lease acquired by instance ID", true)]
        [InlineData("Host lock lease acquired by instance id", true)]
        [InlineData("Host lock lease", false)]
        public void IsEnabled_Tests(string formattedMessage, bool expected)
        {
            ColoredConsoleLogger coloredConsoleLogger = new ColoredConsoleLogger("test", new LoggingFilterOptions(true));
            Assert.Equal(expected, coloredConsoleLogger.DoesMessageStartsWithWhiteListedPrefix(formattedMessage));
        }
    }
}
