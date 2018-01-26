using System;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.Process.AniDb
{
    internal class AniDbSource : ISource
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly INewPluginConfiguration _pluginConfiguration;
        private readonly ITitleSelector _titleSelector;

        public AniDbSource(IAniDbClient aniDbClient, IEpisodeMatcher episodeMatcher,
            INewPluginConfiguration pluginConfiguration, ITitleSelector titleSelector)
        {
            _aniDbClient = aniDbClient;
            _episodeMatcher = episodeMatcher;
            _pluginConfiguration = pluginConfiguration;
            _titleSelector = titleSelector;
        }

        public string Name => "AniDb";

        public OptionAsync<ISourceData> LookupAsync(IMediaItem mediaItem)
        {
            throw new NotImplementedException();
        }

        public OptionAsync<ISourceData> LookupAsync(EmbyItemData embyItemData)
        {
            switch (embyItemData.ItemType)
            {
                case ItemType.Series:

                    return _aniDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                        .BindAsync(s =>
                        {
                            var title = _titleSelector.SelectTitle(s.Titles, _pluginConfiguration.TitlePreference,
                                    embyItemData.Language)
                                .AsTask();

                            return title.MapAsync(t => (ISourceData)new SourceData<AniDbSeriesData>(this, s.Id,
                                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, t.Title), s));
                        });

                case ItemType.Season:

                    var seasonIdentifier = new ItemIdentifier(embyItemData.Identifier.Index.IfNone(1),
                        embyItemData.Identifier.ParentIndex, embyItemData.Identifier.Name);

                    return OptionAsync<ISourceData>.Some(new IdentifierOnlySourceData(this, Option<int>.None,
                        seasonIdentifier));

                case ItemType.Episode:

                    var seriesId = embyItemData.GetParentId(ItemType.Series, this);

                    return seriesId.BindAsync(id =>
                    {
                        var aniDbEpisodeData = _aniDbClient.GetSeriesAsync(id)
                            .BindAsync(series => _episodeMatcher.FindEpisode(series.Episodes,
                                embyItemData.Identifier.ParentIndex,
                                embyItemData.Identifier.Index, embyItemData.Identifier.Name));

                        return aniDbEpisodeData.Bind(e =>
                        {
                            var title = _titleSelector.SelectTitle(e.Titles, _pluginConfiguration.TitlePreference,
                                    embyItemData.Language)
                                .AsTask();

                            return title.MapAsync(t => (ISourceData)new SourceData<AniDbEpisodeData>(this, e.Id,
                                new ItemIdentifier(e.EpisodeNumber.Number, e.EpisodeNumber.SeasonNumber, t.Title), e));
                        });
                    });

                default:
                    return OptionAsync<ISourceData>.None;
            }
        }
    }
}