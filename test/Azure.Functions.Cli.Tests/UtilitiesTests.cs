using Microsoft.Extensions.Logging;
using Xunit;

namespace Azure.Functions.Cli.Tests
{
    public class UtilitiesTests
    {
        [Theory]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Default\": \"None\"}}}", LogLevel.None)]
        [InlineData("{\"version\": \"2.0\",\"logging\": {\"logLevel\": {\"Default\": \"NONE\"}}}", LogLevel.None)]
        [InlineData("{\"version\": \"2.0\",\"logging\": {\"logLevel\": {\"Default\": \"Debug\"}}}", LogLevel.Debug)]
        [InlineData("{\"version\": \"2.0\"}", LogLevel.Information)]
        public void GetHostJsonDefaultLogLevel_Test(string hostJsonContent, LogLevel expectedLogLevel)
        {
            LogLevel actualLogLevel = Utilities.GetHostJsonDefaultLogLevel(hostJsonContent);
            Assert.Equal(actualLogLevel, expectedLogLevel);
        }

        [Theory]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.General\": \"Debug\"}}}", "Host.General",  true)]
        [InlineData("{\"version\": \"2.0\",\"Logging\": {\"LogLevel\": {\"Host.Startup\": \"Debug\"}}}", "Host.General", false)]
        [InlineData("{\"version\": \"2.0\"}", "Function.HttpFunction", false)]
        public void LogLevelExists_Test(string hostJsonContent, string category, bool expected)
        {
            Assert.Equal(expected, Utilities.LogLevelExists(hostJsonContent, category));
        }

        [Theory]
        [InlineData(LogLevel.None, false)]
        [InlineData(LogLevel.Debug, true)]
        [InlineData(LogLevel.Information, true)]
        public void UserLoggingFilter_Test(LogLevel inputLogLevel, bool expected)
        {
            Assert.Equal(expected, Utilities.UserLoggingFilter(inputLogLevel));
        }

        [Theory]
        [InlineData("Function.Function1", LogLevel.None, true)]
        [InlineData("Function.Function1", LogLevel.Warning, true)]
        [InlineData("Function.Function1.User", LogLevel.Information, true)]
        [InlineData("Host.General", LogLevel.Information, false)]
        [InlineData("Host.Startup", LogLevel.Error, true)]
        [InlineData("Host.General", LogLevel.Warning, true)]
        public void DefaultLoggingFilter_Test(string inputCategory, LogLevel inputLogLevel, bool expected)
        {
            Assert.Equal(expected, Utilities.DeafaultLoggingFilter(inputCategory, inputLogLevel, LogLevel.Information, LogLevel.Warning));
        }
    }
}
