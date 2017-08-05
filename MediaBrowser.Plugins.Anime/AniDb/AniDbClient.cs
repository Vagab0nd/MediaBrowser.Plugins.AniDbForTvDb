using System.Text.RegularExpressions;
using System.Threading;
using MediaBrowser.Common.Configuration;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient
    {
        private const string SeriesDataFile = "series.xml";


        // AniDB has very low request rate limits, a minimum of 2 seconds between requests, and an average of 4 seconds between requests
        public static readonly SemaphoreSlim ResourcePool = new SemaphoreSlim(1, 1);

        private static readonly int[] IgnoredCategoryIds =
            { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

        private static readonly Regex AniDbUrlRegex = new Regex(@"http://anidb.net/\w+ \[(?<name>[^\]]*)\]");
        private readonly AniDbFileCache _fileCache;


        public AniDbClient(IApplicationPaths applicationPaths)
        {
            _fileCache = new AniDbFileCache(applicationPaths);
        }
    }
}