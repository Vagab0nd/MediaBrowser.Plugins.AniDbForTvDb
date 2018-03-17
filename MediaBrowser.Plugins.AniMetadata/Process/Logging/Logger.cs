using System;
using System.Collections.Immutable;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.Process.Logging
{
    internal class Logger : ILog
    {
        private readonly ImmutableQueue<LogMessage> _messages = ImmutableQueue<LogMessage>.Empty;

        public IImmutableQueue<LogMessage> Messages => _messages;

        public TResult Log<TResult>(TResult result, string source, string message,
            LogSeverity severity = LogSeverity.Debug)
        {
            _messages.Enqueue(new LogMessage(source, message, severity));

            return result;
        }

        public Func<string, LogSeverity, TResult, TResult> Context<TResult>(string source)
        {
            return (m, s, r) => Log(r, source, m, s);
        }
    }
}