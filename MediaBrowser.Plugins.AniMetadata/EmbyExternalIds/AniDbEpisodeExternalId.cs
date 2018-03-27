using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;

namespace MediaBrowser.Plugins.AniMetadata.EmbyExternalIds
{
    public class AniDbEpisodeExternalId : IExternalId
    {
        public bool Supports(IHasProviderIds item)
        {
            return item is Episode;
        }

        public string Name => SourceNames.AniDb;

        public string Key => SourceNames.AniDb;

        public string UrlFormatString => "http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
    }
}