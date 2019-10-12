using System.IO;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    using Infrastructure;

    internal class TvDbSeriesFileSpec : ILocalFileSpec<TvDbSeriesData>
    {
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly string rootPath;
        private readonly int tvDbSeriesId;

        public TvDbSeriesFileSpec(ICustomJsonSerialiser jsonSerialiser, string rootPath, int tvDbSeriesId)
        {
            this.jsonSerialiser = jsonSerialiser;
            this.rootPath = rootPath;
            this.tvDbSeriesId = tvDbSeriesId;
        }

        public string LocalPath => Path.Combine(this.rootPath, $"anidb\\tvdb\\{this.tvDbSeriesId}.json");

        public ISerialiser Serialiser => this.jsonSerialiser;
    }
}