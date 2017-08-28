using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Titles;
using MediaBrowser.Plugins.Anime.Files;
using MediaBrowser.Plugins.Anime.Providers.AniDb2;
using MediaBrowser.Plugins.Anime.TvDb;

namespace MediaBrowser.Plugins.Anime
{
    public class DependencyConfiguration : IDependencyModule
    {
        public void BindDependencies(IDependencyContainer container)
        {
            Bind<IAniDbClient, AniDbClient>(container);
            Bind<IAniDbDataCache, AniDbDataCache>(container);
            Bind<IFileCache, FileCache>(container);
            Bind<IFileDownloader, FileDownloader>(container);
            Bind<IXmlSerialiser, XmlSerialiser>(container);
            Bind<IAnimeMappingListFactory, AnimeMappingListFactory>(container);
            Bind<IEmbyMetadataFactory, EmbyMetadataFactory>(container);
            Bind<ITitleSelector, TitleSelector>(container);
            Bind<ISeriesTitleCache, SeriesTitleCache>(container);
            Bind<ITitleNormaliser, TitleNormaliser>(container);
            Bind<IEpisodeMatcher, EpisodeMatcher>(container);
            Bind<ITvDbClient, TvDbClient>(container);
            Bind<ICustomJsonSerialiser, JsonSerialiser>(container);
            Bind<ITvDbConnection, TvDbConnection>(container);
            container.RegisterSingleInstance(() => Plugin.Instance.Configuration);
            container.RegisterSingleInstance(() => RateLimiters.Instance);
        }

        private void Bind<TInterface, TImplementation>(IDependencyContainer container)
            where TImplementation : TInterface
        {
            container.Register(typeof(TInterface), typeof(TImplementation));
        }
    }
}