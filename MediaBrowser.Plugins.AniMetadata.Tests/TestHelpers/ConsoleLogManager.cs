using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using NSubstitute;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class ConsoleLogManager : ILogManager
    {
        private readonly ILogger _logger;

        public ConsoleLogManager()
        {
            _logger = Substitute.For<ILogger>();
            _logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine($"Debug: {c.Arg<string>()}"));
            _logger.WhenForAnyArgs(l => l.Info(null, null)).Do(c => Console.WriteLine($"Info: {c.Arg<string>()}"));
            _logger.WhenForAnyArgs(l => l.Error(null, null)).Do(c => Console.WriteLine($"Error: {c.Arg<string>()}"));
            _logger.WhenForAnyArgs(l => l.ErrorException(null, null))
                .Do(c => Console.WriteLine($"Error Exception: {c.Arg<string>()} - {c.Arg<Exception>()}"));
        }

        public ILogger GetLogger(string name)
        {
            return _logger;
        }

        public void ReloadLogger(LogSeverity severity)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void AddConsoleOutput()
        {
            throw new NotImplementedException();
        }

        public void RemoveConsoleOutput()
        {
            throw new NotImplementedException();
        }

        public Task ReloadLogger(LogSeverity severity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public LogSeverity LogSeverity { get; set; }

        public string ExceptionMessagePrefix { get; set; }

        public event EventHandler LoggerLoaded;
    }
}