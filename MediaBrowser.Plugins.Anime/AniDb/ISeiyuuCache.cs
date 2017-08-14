using System.Collections.Generic;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface ISeiyuuCache
    {
        void Add(IEnumerable<SeiyuuData> seiyuu);

        IEnumerable<SeiyuuData> GetAll();
    }
}