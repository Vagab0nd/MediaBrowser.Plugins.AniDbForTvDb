using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal class SeriesMetadataMappingFactory : ISeriesMetadataMappingFactory
    {
        private readonly AniDbSeriesMetadataMappings _aniDbSeriesMetadataMappings;
        private readonly TvDbSeriesMetadataMappings _tvDbSeriesMetadataMappings;

        public SeriesMetadataMappingFactory(AniDbSeriesMetadataMappings aniDbSeriesMetadataMappings,
            TvDbSeriesMetadataMappings tvDbSeriesMetadataMappings)
        {
            _aniDbSeriesMetadataMappings = aniDbSeriesMetadataMappings;
            _tvDbSeriesMetadataMappings = tvDbSeriesMetadataMappings;
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool moveExcessGenresToTags,
            bool addAnimeGenre)
        {
            return _aniDbSeriesMetadataMappings.GetSeriesMappings(maxGenres, addAnimeGenre)
                .Concat(_tvDbSeriesMetadataMappings.GetSeriesMappings(maxGenres, moveExcessGenresToTags));
        }
    }
}