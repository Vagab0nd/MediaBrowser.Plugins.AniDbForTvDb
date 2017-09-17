using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    internal interface ISeriesIdMapper
    {
        SeriesIds MapAniDbId(int aniDbId);

        SeriesIds MapTvDbId(int aniDbId);
    }
}