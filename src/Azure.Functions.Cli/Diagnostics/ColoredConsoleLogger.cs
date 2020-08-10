using System;
using System.Collections.Generic;
using Colors.Net;
using Microsoft.Azure.WebJobs.Script;
using Microsoft.Extensions.Logging;
using Azure.Functions.Cli.Common;
using static Azure.Functions.Cli.Common.OutputTheme;
using System.Linq;
using System.Diagnostics.Tracing;

namespace Azure.Functions.Cli.Diagnostics
{
    public class ColoredConsoleLogger : ILogger
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly bool _verboseErrors;
        private readonly string _category;
        private readonly LoggingFilterOptions _loggingFilterOptions;
        private readonly string[] whitelistedLogsPrefixes = new string[] { "Worker process started and initialized.", "Host lock lease acquired by instance ID" };

        public ColoredConsoleLogger(string category, LoggingFilterOptions loggingFilterOptions)
        {
            _category = category;
            _loggingFilterOptions = loggingFilterOptions;
            _verboseErrors = StaticSettings.IsDebug;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _loggingFilterOptions.IsEnabled(_category, logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string formattedMessage = formatter(state, exception);

            if (string.IsNullOrEmpty(formattedMessage))
            {
                return;
            }

            if (DoesMessageStartsWithWhiteListedPrefix(formattedMessage))
            {
                LogToConsole(logLevel, exception, formattedMessage);
                return;
            }

            if (!IsEnabled(logLevel))
            {
                return;
            }

            LogToConsole(logLevel, exception, formattedMessage);
        }

        private void LogToConsole(LogLevel logLevel, Exception exception, string formattedMessage)
        {
            foreach (var line in GetMessageString(logLevel, formattedMessage, exception))
            {
                var outputline = line.ToString();
                if (_loggingFilterOptions.VerboseLogging)
                {
                    outputline = $"[{DateTime.UtcNow}] {line}";
                }
                ColoredConsole.WriteLine($"{outputline}");
            }
        }

        internal bool DoesMessageStartsWithWhiteListedPrefix(string formattedMessage)
        {
            if (formattedMessage == null)
            {
                throw new ArgumentNullException(nameof(formattedMessage));
            }
            var formattedMessagesWithWhiteListePrefixes = whitelistedLogsPrefixes.Where(s => formattedMessage.StartsWith(s, StringComparison.OrdinalIgnoreCase));
            return formattedMessagesWithWhiteListePrefixes.Any();
        }

        private IEnumerable<RichString> GetMessageString(LogLevel level, string formattedMessage, Exception exception)
        {
            if (exception != null)
            {
                formattedMessage += Environment.NewLine + (_verboseErrors ? exception.ToString() : Utility.FlattenException(exception));
            }

            switch (level)
            {
                case LogLevel.Error:
                    return SplitAndApply(formattedMessage, ErrorColor);
                case LogLevel.Warning:
                    return SplitAndApply(formattedMessage, WarningColor);
                case LogLevel.Information:
                    return SplitAndApply(formattedMessage, AdditionalInfoColor);
                case LogLevel.Debug:
                case LogLevel.Trace:
                    return SplitAndApply(formattedMessage, VerboseColor);
                default:
                    return SplitAndApply(formattedMessage);
            }
        }

        private static IEnumerable<RichString> SplitAndApply(string message, Func<string, RichString> Color = null)
        {
            foreach (var line in message.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                yield return Color == null ? new RichString(line) : Color(line);
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
