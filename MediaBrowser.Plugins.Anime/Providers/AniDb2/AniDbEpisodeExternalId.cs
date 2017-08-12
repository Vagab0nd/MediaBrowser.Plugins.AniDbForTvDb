using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public class AniDbEpisodeExternalId : IExternalId
    {
        public bool Supports(IHasProviderIds item)
        {
            return item is Episode;
        }

        public string Name => "AniDB";

        public string Key => ProviderNames.AniDb;

        public string UrlFormatString => "http://anidb.net/perl-bin/animedb.pl?show=ep&eid={0}";
    }
}