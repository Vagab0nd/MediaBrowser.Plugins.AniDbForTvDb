using System;
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
            _logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine(c.Arg<string>()));
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

        public LogSeverity LogSeverity { get; set; }

        public string ExceptionMessagePrefix { get; set; }

        public event EventHandler LoggerLoaded;
    }
}