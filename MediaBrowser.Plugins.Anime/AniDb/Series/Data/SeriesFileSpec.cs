using System.IO;

namespace MediaBrowser.Plugins.Anime.AniDb.Series.Data
{
    internal class SeriesFileSpec : AniDbFileSpec<AniDbSeriesData>
    {
        private const string ClientName = "mediabrowser";
        private const string SeriesPath = "anidb\\series";
        private readonly string _rootPath;

        public SeriesFileSpec(IXmlFileParser xmlFileParser, string rootPath, int aniDbSeriesId) : base(xmlFileParser)
        {
            _rootPath = rootPath;
            const string seriesQueryUrl =
                "http://api.anidb.net:9001/httpapi?request=anime&client={0}&clientver=1&protover=1&aid={1}";

            Url = string.Format(seriesQueryUrl, ClientName, aniDbSeriesId);
            DestinationFilePath = GetSeriesCacheFilePath(aniDbSeriesId);
        }

        public override string Url { get; }

        public override string DestinationFilePath { get; }

        public override bool IsGZipped => true;
        
        private string GetSeriesCacheFilePath(int aniDbSeriesId)
        {
            return Path.Combine(_rootPath, SeriesPath, aniDbSeriesId.ToString(), "series.xml");
        }
    }
}