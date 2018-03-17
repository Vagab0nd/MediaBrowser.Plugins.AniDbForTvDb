using System;
using System.Collections.Immutable;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.Process.Logging
{
    internal interface ILog
    {
        IImmutableQueue<LogMessage> Messages { get; }

        TResult Log<TResult>(TResult result, string source, string message, LogSeverity severity = LogSeverity.Debug);

        Func<string, LogSeverity, TResult, TResult> Context<TResult>(string source);
    }
}