using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Mapping;
using MediaBrowser.Plugins.Anime.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Converter
{
    public class AnidbConverter
    {
        public AnidbConverter(IApplicationPaths paths, ILogManager logManager)
        {
            var data = Path.Combine(paths.CachePath, "anidb");
            Directory.CreateDirectory(data);

            var mappingPath = Path.Combine(data, "anime-list.xml");
            var downloader = new AnimeMappingListFactory(mappingPath);
            var animelist = downloader.CreateMappingListAsync().Result;

            Mapper = new Mapper(logManager, animelist);
        }

        public Mapper Mapper { get; }
    }
}