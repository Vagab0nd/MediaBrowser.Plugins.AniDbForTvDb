using Emby.AniDbMetaStructure.Process.Sources;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Emby.AniDbMetaStructure.EmbyExternalIds
{
    public class AniDbSeriesExternalId : IExternalId
    {
        public bool Supports(IHasProviderIds item)
        {
            return item is Series;
        }

        public string Name => SourceNames.AniDb;

        public string Key => SourceNames.AniDb;

        public string UrlFormatString => "http://anidb.net/perl-bin/animedb.pl?show=anime&aid={0}";
    }
}