using System.Collections.Generic;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Metadata
{
    public class AniDbPersonInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Image { get; set; }
    }

    public class CastList
    {
        public CastList()
        {
            Cast = new List<AniDbPersonInfo>();
        }

        public List<AniDbPersonInfo> Cast { get; set; }
    }
}