using System;
using System.Collections.Generic;
using MediaBrowser.Common;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using Newtonsoft.Json;
using SimpleInjector;

namespace MediaBrowser.Plugins.AniMetadata
{
    public class DependencyConfiguration
    {
        private static bool _areAniMetadataDependenciesRegistered;

        public static Container Container { get; } = new Container();

        public static T Resolve<T>(IApplicationHost applicationHost) where T : class
        {
            SetUpContainer(applicationHost);

            return Container.GetInstance<T>();
        }

        public static object Resolve(Type type, IApplicationHost applicationHost)
        {
            SetUpContainer(applicationHost);

            return Container.GetInstance(type);
        }

        private static void SetUpContainer(IApplicationHost applicationHost)
        {
            if (!_areAniMetadataDependenciesRegistered)
            {
                SetUpAniMetadataDependencies(Container, applicationHost);
                _areAniMetadataDependenciesRegistered = true;
            }
        }

        private static void SetUpAniMetadataDependencies(Container container, IApplicationHost applicationHost)
        {
            container.Register<EpisodeProvider>();
            container.Register<ImageProvider>();
            container.Register<PersonImageProvider>();
            container.Register<PersonProvider>();
            container.Register<SeasonProvider>();
            container.Register<SeriesProvider>();

            container.Register<AniDbEpisodeProvider>();
            container.Register<AniDbImageProvider>();
            container.Register<AniDbPersonImageProvider>();
            container.Register<AniDbPersonProvider>();
            container.Register<AniDbSeasonProvider>();
            container.Register<AniDbSeriesProvider>();

            container.Register<IAniDbClient, AniDbClient>();
            container.Register<IAniDbDataCache, AniDbDataCache>();
            container.Register<IFileCache, FileCache>();
            container.Register<IFileDownloader, FileDownloader>();
            container.Register<IXmlSerialiser, XmlSerialiser>();
            container.Register<IAnimeMappingListFactory, AnimeMappingListFactory>();
            container.Register<IEpisodeMetadataFactory, AniDbEpisodeMetadataFactory>();
            container.Register<ISeasonMetadataFactory, AniDbSeasonMetadataFactory>();
            container.Register<ISeriesMetadataFactory, AniDbSeriesMetadataFactory>();
            container.Register<ITitleSelector, TitleSelector>();
            container.Register<ISeriesTitleCache, SeriesTitleCache>();
            container.Register<ITitleNormaliser, TitleNormaliser>();
            container.Register<IEpisodeMatcher, EpisodeMatcher>();
            container.Register<ITvDbClient, TvDbClient>();
            container.Register<ICustomJsonSerialiser, JsonSerialiser>();
            container.Register<ITvDbConnection, TvDbConnection>();
            container.Register<ISeriesDataLoader, AniDbSeriesDataLoader>();
            container.Register<IAniDbParser, AniDbParser>();
            container.Register<IPluginConfiguration, AniMetadataConfiguration>();
            container.Register<IMappingConfiguration, MappingConfiguration>();
            container.Register<IDataMapperFactory, DataMapperFactory>();
            container.Register<IEpisodeMapper, EpisodeMapper>();
            container.Register<IDefaultSeasonEpisodeMapper, DefaultSeasonEpisodeMapper>();
            container.Register<IGroupMappingEpisodeMapper, GroupMappingEpisodeMapper>();
            container.Register<AniDbSourceMappingConfiguration>();
            container.Register<TvDbSourceMappingConfiguration>();
            container.Register<IEnumerable<ISourceMappingConfiguration>, SourceMappingConfigurations>();

            container.Register(() => Plugin.Instance.Configuration, Lifestyle.Singleton);
            container.Register(() => RateLimiters.Instance, Lifestyle.Singleton);

            Container.ResolveUnregisteredType += (sender, args) =>
            {
                args.Register(Lifestyle.Singleton.CreateRegistration(args.UnregisteredServiceType,
                    GetResolveMethod(args.UnregisteredServiceType, applicationHost), container));
            };

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
        }

        private static Func<object> GetResolveMethod(Type type, IApplicationHost applicationHost)
        {
            var interfaceMethod = typeof(IApplicationHost).GetMethod(nameof(IApplicationHost.Resolve));
            var interfaceMap = applicationHost.GetType().GetInterfaceMap(typeof(IApplicationHost));

            var implementationMethodIndex = Array.IndexOf(interfaceMap.InterfaceMethods, interfaceMethod);

            return () => interfaceMap.TargetMethods[implementationMethodIndex]
                .MakeGenericMethod(type)
                .Invoke(applicationHost, null);
        }
    }
}