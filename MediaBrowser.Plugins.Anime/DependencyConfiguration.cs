using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.Providers.AniDb2;

namespace MediaBrowser.Plugins.Anime
{
    public class DependencyConfiguration : IDependencyModule
    {
        public void BindDependencies(IDependencyContainer container)
        {
            Bind<IAniDbClient, AniDbClient>(container);
            Bind<IAniDbDataCache, AniDbDataCache>(container);
            Bind<IAniDbFileCache, AniDbFileCache>(container);
            Bind<IFileDownloader, FileDownloader>(container);
            Bind<IAniDbFileParser, AniDbFileParser>(container);
            Bind<IAnimeMappingListFactory, AnimeMappingListFactory>(container);
            Bind<IEmbyMetadataFactory, EmbyMetadataFactory>(container);
            Bind<ITitleSelector, TitleSelector>(container);
            container.RegisterSingleInstance(() => Plugin.Instance.Configuration);
        }

        private void Bind<TInterface, TImplementation>(IDependencyContainer container)
        {
            container.Register(typeof(TInterface), typeof(TImplementation));
        }
    }
}