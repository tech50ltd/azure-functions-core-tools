using Azure.Functions.Cli.Common;
using Azure.Functions.Cli.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Azure.Functions.Cli.Tests
{
    public class LoggingFilterOptionsTests
    {
        [Theory]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"None\"}}}", true, LogLevel.None, true)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"DEBUG\"}}}", true, LogLevel.Debug, true)]
        [InlineData("{\"version\": \"2.0\"}", false, LogLevel.Information, false)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", true, LogLevel.Information, false)]
        public void LoggingFilterOptions_Tests(string hostJsonContent, bool verboseLogging, LogLevel expectedDefaultLogLevel, bool defaultLogLevelExists)
        {
            FileSystemHelpers.WriteAllTextToFile(Constants.HostJsonFileName, hostJsonContent);
            LoggingFilterOptions loggingFilterOptions = new LoggingFilterOptions(verboseLogging);
            Assert.Equal(verboseLogging, loggingFilterOptions.VerboseLogging);
            Assert.Equal(defaultLogLevelExists, loggingFilterOptions.DefaultLogLevelExists);
            Assert.Equal(loggingFilterOptions.DefaultLogLevel, expectedDefaultLogLevel);
            Assert.Equal(LogLevel.Information, loggingFilterOptions.UserLogDefaultLogLevel);
            if (verboseLogging)
            {
                Assert.Equal(LogLevel.Information, loggingFilterOptions.SystemLogDefaultLogLevel);
            }
            else
            {
                Assert.Equal(LogLevel.Warning, loggingFilterOptions.SystemLogDefaultLogLevel);
            }
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                foreach (var service in builder.Services)
                {
                    Console.WriteLine($"Service: {service.ServiceType.FullName}\n Lifetime: {service.Lifetime}\n Instance: {service.ImplementationType?.FullName}");
                }
                loggingFilterOptions.AddConsoleLoggingProvider(builder);
                foreach (var service in builder.Services)
                {
                    Console.WriteLine($"Service: {service.ServiceType.FullName}\n Lifetime: {service.Lifetime}\n Instance: {service.ImplementationType?.FullName}");
                }
                var serviceProvider = builder.Services.BuildServiceProvider();

                // This will succeed.
                var coloredConsoleLoggerProvider = (ColoredConsoleLoggerProvider)serviceProvider.GetService<ILoggerProvider>();
                Assert.NotNull(coloredConsoleLoggerProvider);
            });
        }

        [Theory]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"None\"}}}", "test",  LogLevel.Information, false)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", "Host.Startup", LogLevel.Information, true)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", "Host.General", LogLevel.Information, false)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", "Host.General", LogLevel.Warning, true)]
        public void IsEnabled_Tests(string hostJsonContent, string category, LogLevel logLevel, bool expected)
        {
            FileSystemHelpers.WriteAllTextToFile(Constants.HostJsonFileName, hostJsonContent);
            LoggingFilterOptions loggingFilterOptions = new LoggingFilterOptions(false);
            Assert.Equal(expected, loggingFilterOptions.IsEnabled(category, logLevel));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IsCI_Tests(bool isCiEnv)
        {
            if (isCiEnv)
            {
                Environment.SetEnvironmentVariable(LoggingFilterOptions.Ci_Build_Number, "90l99");
            }
            LoggingFilterOptions loggingFilterOptions = new LoggingFilterOptions(false);
            Assert.Equal(isCiEnv, loggingFilterOptions.IsCiEnvironment());
            Environment.SetEnvironmentVariable(LoggingFilterOptions.Ci_Build_Number, "");
        }
    }
}
