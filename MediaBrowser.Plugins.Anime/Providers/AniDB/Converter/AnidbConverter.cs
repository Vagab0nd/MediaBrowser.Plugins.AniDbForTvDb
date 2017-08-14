using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;
using MediaBrowser.Plugins.Anime.Mapping;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Converter
{
    public class AnidbConverter
    {
        public AnidbConverter(IApplicationPaths paths, ILogManager logManager)
        {
            var data = Path.Combine(paths.CachePath, "anidb");
            Directory.CreateDirectory(data);

            var mappingPath = Path.Combine(data, "anime-list.xml");
            var animelist = new AnimeMappingListData();

            Mapper = new Mapper(logManager, animelist);
        }

        public Mapper Mapper { get; }
    }
}