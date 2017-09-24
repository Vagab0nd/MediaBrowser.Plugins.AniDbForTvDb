using System.Linq;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeasonMetadataFactory : ISeasonMetadataFactory
    {
        private readonly IAniDbParser _aniDbParser;
        private readonly PluginConfiguration _configuration;
        private readonly ITitleSelector _titleSelector;

        public AniDbSeasonMetadataFactory(ITitleSelector titleSelector, IAniDbParser aniDbParser,
            PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _aniDbParser = aniDbParser;
            _configuration = configuration;
        }

        public MetadataResult<Season> NullSeasonResult => new MetadataResult<Season>();

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeriesData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeasonResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Season>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeason(aniDbSeriesData, seasonIndex, t.Title)
                },
                () => { });

            return metadataResult;
        }

        private Season CreateEmbySeason(AniDbSeriesData aniDbSeriesData, int seasonIndex, string selectedTitle)
        {
            var embySeason = new Season
            {
                Name = selectedTitle,
                Overview = _aniDbParser.FormatDescription(aniDbSeriesData.Description),
                PremiereDate = aniDbSeriesData.StartDate,
                EndDate = aniDbSeriesData.EndDate,
                CommunityRating = aniDbSeriesData.Ratings.OfType<PermanentRatingData>().Single().Value,
                IndexNumber = seasonIndex
            };

            embySeason.Studios = _aniDbParser.GetStudios(aniDbSeriesData).ToArray();
            embySeason.Genres.AddRange(_aniDbParser.GetGenres(aniDbSeriesData, _configuration.MaxGenres, _configuration.AddAnimeGenre));
            embySeason.Tags = _aniDbParser.GetTags(aniDbSeriesData, _configuration.MaxGenres, _configuration.AddAnimeGenre).ToArray();

            return embySeason;
        }
    }
}