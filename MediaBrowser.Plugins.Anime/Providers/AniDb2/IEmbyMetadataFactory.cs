using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public interface IEmbyMetadataFactory
    {
        MetadataResult<Series> NullSeriesResult { get; }

        MetadataResult<Season> NullSeasonResult { get; }

        MetadataResult<Episode> NullEpisodeResult { get; }

        MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeries aniDbSeries, string metadataLanguage);

        MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeries aniDbSeries, int seasonIndex, string metadataLanguage);

        MetadataResult<Episode> CreateEpisodeMetadataResult(AniDbEpisode aniDbEpisode,
            DiscriminatedUnion<AniDbMapper.TvDbEpisodeNumber, AniDbMapper.AbsoluteEpisodeNumber,
                AniDbMapper.UnmappedEpisodeNumber> tvDbEpisode, string metadataLanguage);
    }
}