using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class AniListSource : IAniListSource
    {
        private readonly IAniListNameSelector _aniListNameSelector;
        private readonly IEnumerable<IEmbySourceDataLoader> _embySourceDataLoaders;
        private readonly ITitlePreferenceConfiguration _titlePreferenceConfiguration;

        public AniListSource(ITitlePreferenceConfiguration titlePreferenceConfiguration,
            IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders, IAniListNameSelector aniListNameSelector)
        {
            _titlePreferenceConfiguration = titlePreferenceConfiguration;
            _embySourceDataLoaders = embySourceDataLoaders;
            _aniListNameSelector = aniListNameSelector;
        }

        public SourceName Name => SourceNames.AniList;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return _embySourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, "", mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public Either<ProcessFailedResult, string> SelectTitle(AniListTitleData titleData,
            string metadataLanguage, ProcessResultContext resultContext)
        {
            return _aniListNameSelector
                .SelectTitle(titleData, _titlePreferenceConfiguration.TitlePreference, metadataLanguage)
                .ToEither(resultContext.Failed("Failed to find a title"));
        }
    }
}