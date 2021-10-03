using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;

namespace Emby.AniDbMetaStructure.AniDb.SeriesData
{
    internal class SeriesFileSpec : IRemoteFileSpec<AniDbSeriesData>
    {
        private const string ClientName = "mediabrowser";
        private const string SeriesPath = "anidb\\series";
        private readonly string rootPath;
        private readonly IXmlSerialiser serializer;

        public SeriesFileSpec(string rootPath, int aniDbSeriesId, IXmlSerialiser serializer)
        {
            this.rootPath = rootPath;
            this.serializer = serializer;
            const string seriesQueryUrl =
                "http://api.anidb.net:9001/httpapi?request=anime&client={0}&clientver=1&protover=1&aid={1}";

            this.Url = string.Format(seriesQueryUrl, ClientName, aniDbSeriesId);
            this.LocalPath = this.GetSeriesCacheFilePath(aniDbSeriesId);
        }

        public string Url { get; }

        public string LocalPath { get; }

        public bool IsGZipped => true;

        public ISerialiser Serialiser => this.serializer;

        private string GetSeriesCacheFilePath(int aniDbSeriesId)
        {
            return Path.Combine(this.rootPath, SeriesPath, aniDbSeriesId.ToString(), "series.xml");
        }
    }
}