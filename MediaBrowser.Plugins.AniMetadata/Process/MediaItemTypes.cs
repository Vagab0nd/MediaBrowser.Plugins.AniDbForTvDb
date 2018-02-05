using MediaBrowser.Controller.Entities.TV;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal static class MediaItemTypes
    {
        internal static readonly IMediaItemType Episode =
            new MediaItemType<Episode>(MediaItemTypeValue.Episode, (c, l) => c.GetEpisodeMetadataMapping(l));

        internal static readonly IMediaItemType Season =
            new MediaItemType<Season>(MediaItemTypeValue.Season, (c, l) => c.GetSeasonMetadataMapping(l));

        internal static readonly IMediaItemType Series =
            new MediaItemType<Series>(MediaItemTypeValue.Series, (c, l) => c.GetSeriesMetadataMapping(l));
    }
}