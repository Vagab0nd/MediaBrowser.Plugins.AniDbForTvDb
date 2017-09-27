using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Configuration;
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
            void Bind<TInterface, TImplementation>()
                where TImplementation : TInterface
            {
                container.Register(typeof(TInterface), typeof(TImplementation));
            }

            Bind<IAniDbClient, AniDbClient>();
            Bind<IAniDbDataCache, AniDbDataCache>();
            Bind<IFileCache, FileCache>();
            Bind<IFileDownloader, FileDownloader>();
            Bind<IXmlSerialiser, XmlSerialiser>();
            Bind<IAnimeMappingListFactory, AnimeMappingListFactory>();
            Bind<IEpisodeMetadataFactory, AniDbEpisodeMetadataFactory>();
            Bind<ISeasonMetadataFactory, AniDbSeasonMetadataFactory>();
            Bind<ISeriesMetadataFactory, AniDbSeriesMetadataFactory>();
            Bind<ITitleSelector, TitleSelector>();
            Bind<ISeriesTitleCache, SeriesTitleCache>();
            Bind<ITitleNormaliser, TitleNormaliser>();
            Bind<IEpisodeMatcher, EpisodeMatcher>();
            Bind<ITvDbClient, TvDbClient>();
            Bind<ICustomJsonSerialiser, JsonSerialiser>();
            Bind<ITvDbConnection, TvDbConnection>();
            Bind<ISeriesDataLoader, AniDbSeriesDataLoader>();
            Bind<IAniDbParser, AniDbParser>();
            Bind<IPluginConfiguration, AniMetadataConfiguration>();
            Bind<IMappingConfiguration, MappingConfiguration>();

            container.RegisterSingleInstance(() => Plugin.Instance.Configuration);
            container.RegisterSingleInstance(() => new ISourceMappingConfiguration[]
            {
                new AniDbSourceMappingConfiguration(new AniDbParser()),
                new TvDbSourceMappingConfiguration()
            }.AsEnumerable());

            container.RegisterSingleInstance(() => RateLimiters.Instance);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
        }
    }
}