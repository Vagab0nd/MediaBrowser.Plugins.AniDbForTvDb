using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal static class TestProcessResultContext
    {
        public static ProcessResultContext Instance => new ProcessResultContext(string.Empty, string.Empty, null);

        public static ProcessFailedResult Failed(string reason)
        {
            return new ProcessFailedResult(string.Empty, string.Empty, null, reason);
        }
    }
}