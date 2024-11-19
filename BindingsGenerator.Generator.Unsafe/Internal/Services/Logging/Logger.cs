using BindingsGenerator.Core;
using BindingsGenerator.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace BindingsGenerator.Generator.Unsafe.Internal.Services.Logging
{
    internal class Logger : ILogger
    {
        readonly IGenerationLogCollector _logCollector;

        readonly string _name;

        public Logger(
            string name,
            IGenerationLogCollector logCollector)
        {
            _logCollector = logCollector;

            _name = name;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = $"{formatter(state, exception)} ({_name})";
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    _logCollector.LogInfo(message);
                    break;
                case LogLevel.Warning:
                    _logCollector.LogWarning("", message);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    _logCollector.LogError("", message);
                    break;
            }
        }
    }
}
