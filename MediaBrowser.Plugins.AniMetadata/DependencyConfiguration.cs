using System.Collections.Generic;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.AniMetadata
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
            Bind<IEpisodeMetadataFactory, AniDbEpisodeMetadataFactory>(container);
            Bind<ISeasonMetadataFactory, AniDbSeasonMetadataFactory>(container);
            Bind<ISeriesMetadataFactory, AniDbSeriesMetadataFactory>(container);
            Bind<ITitleSelector, TitleSelector>(container);
            Bind<ISeriesTitleCache, SeriesTitleCache>(container);
            Bind<ITitleNormaliser, TitleNormaliser>(container);
            Bind<IEpisodeMatcher, EpisodeMatcher>(container);
            Bind<ITvDbClient, TvDbClient>(container);
            Bind<ICustomJsonSerialiser, JsonSerialiser>(container);
            Bind<ITvDbConnection, TvDbConnection>(container);
            container.RegisterSingleInstance(() => Plugin.Instance.Configuration);
            container.RegisterSingleInstance(() => RateLimiters.Instance);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new MaybeJsonConverter() }
            };
        }

        private void Bind<TInterface, TImplementation>(IDependencyContainer container)
            where TImplementation : TInterface
        {
            container.Register(typeof(TInterface), typeof(TImplementation));
        }
    }
}