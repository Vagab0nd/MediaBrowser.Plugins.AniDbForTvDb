using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.TvDb.Data;

namespace Emby.AniDbMetaStructure.TvDb
{
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