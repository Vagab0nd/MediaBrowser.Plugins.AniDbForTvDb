using System.Collections.Generic;

namespace MediaBrowser.Plugins.Anime.AniDb.Seiyuu
{
    public interface ISeiyuuCache
    {
        void Add(IEnumerable<SeiyuuData> seiyuu);

        IEnumerable<SeiyuuData> GetAll();
    }
}