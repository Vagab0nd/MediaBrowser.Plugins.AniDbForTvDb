using System.Collections.Generic;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface ISeiyuuCache
    {
        void Add(IEnumerable<Seiyuu> seiyuu);

        IEnumerable<Seiyuu> GetAll();
    }
}