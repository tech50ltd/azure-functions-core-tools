using Azure.Functions.Cli.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Functions.Cli.Diagnostics
{
    internal class LoggingBuilder : IConfigureBuilder<ILoggingBuilder>
    {
        private LoggingFilterOptions _loggingFilterOptions;

        public LoggingBuilder(LoggingFilterOptions loggingFilterOptions)
        {
            _loggingFilterOptions = loggingFilterOptions;
        }

        public void Configure(ILoggingBuilder builder)
        {
            _loggingFilterOptions.AddConsoleLoggingProvider(builder);

            builder.Services.AddSingleton<TelemetryClient>(provider =>
            {
                TelemetryConfiguration configuration = provider.GetService<TelemetryConfiguration>();
                TelemetryClient client = new TelemetryClient(configuration);

                client.Context.GetInternalContext().SdkVersion = $"azurefunctionscoretools: {Constants.CliVersion}";

                return client;
            });
        }
    }
}
