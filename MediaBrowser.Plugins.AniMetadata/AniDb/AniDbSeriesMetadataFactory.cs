using System.Linq;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeriesMetadataFactory : ISeriesMetadataFactory
    {
        private readonly IAniDbParser _aniDbParser;
        private readonly PluginConfiguration _configuration;

        private readonly ITitleSelector _titleSelector;

        public AniDbSeriesMetadataFactory(ITitleSelector titleSelector, IAniDbParser aniDbParser,
            PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _aniDbParser = aniDbParser;
            _configuration = configuration;
        }

        public MetadataResult<Series> NullSeriesResult => new MetadataResult<Series>();

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeriesData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeriesResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Series>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeries(aniDbSeriesData, t.Title),
                    People = _aniDbParser.GetPeople(aniDbSeriesData).ToList()
                },
                () => { });

            return metadataResult;
        }

        private Series CreateEmbySeries(AniDbSeriesData aniDbSeriesData, string selectedTitle)
        {
            var embySeries = new Series
            {
                PremiereDate = aniDbSeriesData.StartDate,
                EndDate = aniDbSeriesData.EndDate,
                Name = selectedTitle,
                Overview = _aniDbParser.FormatDescription(aniDbSeriesData.Description),
                CommunityRating = aniDbSeriesData.Ratings.OfType<PermanentRatingData>().Single().Value
            };

            embySeries.ProviderIds.Add(ProviderNames.AniDb, aniDbSeriesData.Id.ToString());

            embySeries.Studios = _aniDbParser.GetStudios(aniDbSeriesData).ToArray();
            embySeries.Genres.AddRange(_aniDbParser.GetGenres(aniDbSeriesData));
            embySeries.Tags = _aniDbParser.GetTags(aniDbSeriesData).ToArray();

            return embySeries;
        }
    }
}