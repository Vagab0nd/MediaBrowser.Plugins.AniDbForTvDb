using System.Collections.Generic;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal interface IPluginConfiguration
    {
        bool AddAnimeGenre { get; set; }

        int MaxGenres { get; set; }

        bool MoveExcessGenresToTags { get; set; }

        TitleType TitlePreference { get; set; }

        LibraryStructure LibraryStructure { get; }

        string TvDbApiKey { get; set; }

        string AniListAuthorisationCode { get; set; }

        /// <summary>
        ///     The source that was used to name the files
        /// </summary>
        ISource FileStructureSource { get; }

        /// <summary>
        ///     The source to use to structure the Emby library
        /// </summary>
        ISource LibraryStructureSource { get; }

        IEnumerable<string> ExcludedSeriesNames { get; }

        IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage);
    }
}