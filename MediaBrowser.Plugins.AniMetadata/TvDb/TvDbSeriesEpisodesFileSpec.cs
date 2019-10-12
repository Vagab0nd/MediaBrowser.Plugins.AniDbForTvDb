using System.IO;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    using Infrastructure;

    internal class TvDbSeriesEpisodesFileSpec : ILocalFileSpec<TvDbEpisodeCollection>
    {
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly string rootPath;
        private readonly int tvDbSeriesId;

        public TvDbSeriesEpisodesFileSpec(ICustomJsonSerialiser jsonSerialiser, string rootPath, int tvDbSeriesId)
        {
            this.jsonSerialiser = jsonSerialiser;
            this.rootPath = rootPath;
            this.tvDbSeriesId = tvDbSeriesId;
        }

        public string LocalPath => Path.Combine(this.rootPath, $"anidb\\tvdb\\{this.tvDbSeriesId}_Episodes.json");

        public ISerialiser Serialiser => this.jsonSerialiser;
    }
}