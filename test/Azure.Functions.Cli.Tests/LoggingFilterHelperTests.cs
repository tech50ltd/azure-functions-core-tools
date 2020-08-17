using Azure.Functions.Cli.Common;
using Azure.Functions.Cli.Diagnostics;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Azure.Functions.Cli.Tests
{
    public class LoggingFilterHelperTests
    {
        private ScriptApplicationHostOptions _hostOptions;

        public LoggingFilterHelperTests()
        {
            _hostOptions = new ScriptApplicationHostOptions
            {
                ScriptPath = Directory.GetCurrentDirectory()
            };
        }

        [Theory]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"None\"}}}", true, true, LogLevel.None, true)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"DEBUG\"}}}", true, true, LogLevel.Debug, true)]
        [InlineData("{\"version\": \"2.0\"}", false, false, LogLevel.Information, false)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", true, true, LogLevel.Information, false)]
        public void LoggingFilterOptions_Tests(string hostJsonContent, bool verboseLogging, bool verboseLoggingExists, LogLevel expectedDefaultLogLevel, bool defaultLogLevelExists)
        {
            FileSystemHelpers.WriteAllTextToFile(Constants.HostJsonFileName, hostJsonContent);
            IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(hostJsonContent))).Build();
            LoggingFilterHelper loggingFilterOptions = new LoggingFilterHelper(configuration, verboseLogging, verboseLoggingExists);
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
            var configuration = Utilities.BuildHostJsonConfigutation(_hostOptions);
            LoggingFilterHelper loggingFilterOptions = new LoggingFilterHelper(configuration, false, false);
            Assert.Equal(expected, loggingFilterOptions.IsEnabled(category, logLevel));
        }

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, true)]
        [InlineData(true, false, true, false)]
        public void IsCI_Tests(bool isCiEnv, bool verboseLogging, bool verboseLoggingArgExists, bool expected)
        {
            if (isCiEnv)
            {
                Environment.SetEnvironmentVariable(LoggingFilterHelper.Ci_Build_Number, "90l99");
            }
            string defaultJson = "{\"version\": \"2.0\"}";
            FileSystemHelpers.WriteAllTextToFile(Constants.HostJsonFileName, defaultJson);
            var configuration = Utilities.BuildHostJsonConfigutation(_hostOptions);
            LoggingFilterHelper loggingFilterOptions = new LoggingFilterHelper(configuration, verboseLogging, verboseLoggingArgExists);
            Assert.Equal(expected, loggingFilterOptions.IsCiEnvironment(verboseLoggingArgExists));
            Environment.SetEnvironmentVariable(LoggingFilterHelper.Ci_Build_Number, "");
        }
    }
}
