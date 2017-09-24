using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    public interface ISeriesMetadataMappingFactory
    {
        IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool moveExcessGenresToTags, bool addAnimeGenre);
    }
}