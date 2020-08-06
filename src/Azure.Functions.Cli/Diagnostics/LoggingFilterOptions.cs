using Azure.Functions.Cli.Common;
using Azure.Functions.Cli.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Functions.Cli
{
    public class LoggingFilterOptions
    {
        private const string DefaultLogLevelKey = "default";
        private readonly string _hostJsonFileContent = string.Empty;

        public LoggingFilterOptions(bool verboseLogging = false)
        {
            VerboseLogging = verboseLogging;
            if (VerboseLogging)
            {
                SystemLogDefaultLogLevel = LogLevel.Information;
            }
            try
            {
                _hostJsonFileContent = FileSystemHelpers.ReadAllTextFromFile(Constants.HostJsonFileName);
                DefaultLogLevelExists = Utilities.LogLevelExists(_hostJsonFileContent, DefaultLogLevelKey);
                if (DefaultLogLevelExists)
                {
                    DefaultLogLevel = Utilities.GetHostJsonDefaultLogLevel(_hostJsonFileContent);
                    SystemLogDefaultLogLevel = DefaultLogLevel;
                    UserLogDefaultLogLevel = DefaultLogLevel;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Default level for system logs
        /// </summary>
        public LogLevel SystemLogDefaultLogLevel { get; } = LogLevel.Warning;

        /// <summary>
        /// Default level for user logs
        /// </summary>
        public LogLevel UserLogDefaultLogLevel { get; } = LogLevel.Information;

        /// <summary>
        /// Default log level set in host.json. If not present, deafaults to Information
        /// </summary>
        public LogLevel DefaultLogLevel { get; private set; } = LogLevel.Information;

        /// <summary>
        /// Is set to true if "default" key is present in "LogLevel" section in host.json. If set, SystemLogDefaultLogLevel is set to Information.
        /// </summary>
        public bool DefaultLogLevelExists { get; private set; }

        /// <summary>
        /// Is set to true if `func start` is with `--verbose` flag. If set, SystemLogDefaultLogLevel is set to Information
        /// </summary>
        public bool VerboseLogging { get; private set; }

        internal void AddConsoleLoggingProvider(ILoggingBuilder loggingBuilder)
        {
            // Filter is needed to force all the logs.
            loggingBuilder.AddProvider(new ColoredConsoleLoggerProvider(this)).AddFilter((category, level) => true);
        }

        internal bool IsEnabled(string category, LogLevel logLevel)
        {
            if (Utilities.LogLevelExists(_hostJsonFileContent, category))
            {
                // If category exists in `loglevel` section, ensure defaults do not apply.
                return Utilities.UserLoggingFilter(logLevel);
            }
            if (DefaultLogLevel == LogLevel.None)
            {
                return false;
            }
            return Utilities.DeafaultLoggingFilter(category, logLevel, UserLogDefaultLogLevel, SystemLogDefaultLogLevel);
        }
    }
}
