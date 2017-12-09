using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public interface IPluginConfiguration
    {
        bool AddAnimeGenre { get; set; }

        int MaxGenres { get; set; }

        bool MoveExcessGenresToTags { get; set; }

        TitleType TitlePreference { get; set; }

        LibraryStructure LibraryStructure { get; }

        string TvDbApiKey { get; set; }

        IEnumerable<string> ExcludedSeriesNames { get; }

        IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage);
    }
}