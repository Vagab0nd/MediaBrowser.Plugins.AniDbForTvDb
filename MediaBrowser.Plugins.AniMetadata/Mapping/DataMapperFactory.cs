using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class DataMapperFactory : IDataMapperFactory
    {
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ILogManager _logManager;
        private readonly ITvDbClient _tvDbClient;

        public DataMapperFactory(ITvDbClient tvDbClient, IEpisodeMatcher episodeMatcher,
            IEpisodeMapper episodeMapper, ILogManager logManager)
        {
            _tvDbClient = tvDbClient;
            _episodeMatcher = episodeMatcher;
            _episodeMapper = episodeMapper;
            _logManager = logManager;
        }

        public IDataMapper GetDataMapper(IMappingList mappingList)
        {
            return new DataMapper(mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);
        }
    }
}