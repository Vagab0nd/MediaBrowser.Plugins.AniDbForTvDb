using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.Process.Logging
{
    internal class LogMessage
    {
        public LogMessage(string source, string message, LogSeverity severity)
        {
            Source = source;
            Message = message;
            Severity = severity;
        }

        public string Source { get; }

        public string Message { get; }

        public LogSeverity Severity { get; }
    }
}