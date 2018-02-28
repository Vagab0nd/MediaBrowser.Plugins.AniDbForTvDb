using System.Threading;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class DataMapperFactory : IDataMapperFactory
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ILogManager _logManager;
        private readonly ITvDbClient _tvDbClient;

        public DataMapperFactory(ITvDbClient tvDbClient, IAniDbClient aniDbClient, IEpisodeMatcher episodeMatcher,
            IEpisodeMapper episodeMapper, ILogManager logManager, IAnimeMappingListFactory animeMappingListFactory)
        {
            _tvDbClient = tvDbClient;
            _aniDbClient = aniDbClient;
            _episodeMatcher = episodeMatcher;
            _episodeMapper = episodeMapper;
            _logManager = logManager;
            _animeMappingListFactory = animeMappingListFactory;
        }

        public IDataMapper GetDataMapper(IMappingList mappingList)
        {
            return new DataMapper(mappingList, _tvDbClient, _aniDbClient, _episodeMatcher, _episodeMapper, _logManager);
        }

        public OptionAsync<IDataMapper> GetDataMapperAsync()
        {
            return _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None)
                .MapAsync(l => (IDataMapper)new DataMapper(l, _tvDbClient, _aniDbClient,
                    _episodeMatcher, _episodeMapper, _logManager));
        }
    }
}