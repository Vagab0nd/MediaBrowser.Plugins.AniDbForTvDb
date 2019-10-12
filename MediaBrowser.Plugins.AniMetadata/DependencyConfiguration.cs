using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Common;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Providers;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using Newtonsoft.Json;
using SimpleInjector;

namespace MediaBrowser.Plugins.AniMetadata
{
    using Infrastructure;

    public class DependencyConfiguration
    {
        private static bool areAniMetadataDependenciesRegistered;

        internal static Container Container { get; private set; } = new Container();

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

        internal static void Reset()
        {
            Container = new Container();
            areAniMetadataDependenciesRegistered = false;
        }

        private static void SetUpContainer(IApplicationHost applicationHost)
        {
            if (!areAniMetadataDependenciesRegistered)
            {
                SetUpAniMetadataDependencies(Container, applicationHost);
                areAniMetadataDependenciesRegistered = true;
            }
        }

        private static void SetUpAniMetadataDependencies(Container container, IApplicationHost applicationHost)
        {
            container.Register<ImageProvider>();
            container.Register<PersonImageProvider>();
            container.Register<PersonProvider>();
            container.Register<SeriesProviderEntryPoint>();

            container.Register<AniDbImageProvider>();
            container.Register<AniDbPersonImageProvider>();
            container.Register<AniDbPersonProvider>();

            container.Register<IAniDbClient, AniDbClient>();
            container.Register<IAniListClient, AniListClient>();
            container.Register<IAniDbDataCache, AniDbDataCache>();
            container.Register<IFileCache, FileCache>();
            container.Register<IFileDownloader, FileDownloader>();
            container.Register<IXmlSerialiser, XmlSerialiser>();
            container.Register<IMappingList, MappingList>();
            container.Register<IAniDbTitleSelector, AniDbTitleSelector>();
            container.Register<IAniListNameSelector, AniListNameSelector>();
            container.Register<ISeriesTitleCache, SeriesTitleCache>();
            container.Register<ITitleNormaliser, TitleNormaliser>();
            container.Register<IAniDbEpisodeMatcher, AniDbEpisodeMatcher>();
            container.Register<ITvDbClient, TvDbClient>();
            container.Register<ICustomJsonSerialiser, JsonSerialiser>();
            container.Register<IJsonConnection, JsonConnection>();
            container.Register<IAniDbParser, AniDbParser>();
            container.Register<IPluginConfiguration, AniMetadataConfiguration>();
            container.Register<ITitlePreferenceConfiguration, AniMetadataConfiguration>();
            container.Register<IAnilistConfiguration, AniMetadataConfiguration>();
            container.Register<IMappingConfiguration, MappingConfiguration>();
            container.Register<IEpisodeMapper, EpisodeMapper>();
            container.Register<IDefaultSeasonEpisodeMapper, DefaultSeasonEpisodeMapper>();
            container.Register<IGroupMappingEpisodeMapper, GroupMappingEpisodeMapper>();
            container.Register<AniDbSourceMappingConfiguration>();
            container.Register<TvDbSourceMappingConfiguration>();
            container.Register<AniListSourceMappingConfiguration>();
            container.Register<IEnumerable<ISourceMappingConfiguration>, SourceMappingConfigurations>();
            container.Register<IMediaItemProcessor, MediaItemProcessor>();
            container.Register<IMediaItemBuilder, MediaItemBuilder>();
            container.Register<ISources, Sources>();

            container.Register<IAniDbSource, AniDbSource>();
            container.Register<ITvDbSource, TvDbSource>();
            container.Register<IAniListSource, AniListSource>();

            container.Register<Func<IAniDbSource>>(() => container.GetInstance<IAniDbSource>);
            container.Register<Func<ITvDbSource>>(() => container.GetInstance<ITvDbSource>);
            container.Register<Func<IAniListSource>>(() => container.GetInstance<IAniListSource>);

            container.Register<SeriesProvider>();
            container.Register<SeasonProvider>();
            container.Register<EpisodeProvider>();

            RegisterCollection<ISource>(container);
            RegisterCollection<ISourceDataLoader>(container);
            RegisterCollection<IEmbySourceDataLoader>(container);

            container.Register<Func<IEnumerable<ISource>>>(() => container.GetInstance<IEnumerable<ISource>>);

            container.Register(() => Plugin.Instance.Configuration, Lifestyle.Singleton);

            container.Register(() => RateLimiters.Instance, Lifestyle.Singleton);
            container.Register<IAniListToken, AniListToken>(Lifestyle.Singleton);

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

        private static void RegisterCollection<TInterface>(Container container) where TInterface : class
        {
            var types = container.GetTypesToRegister(typeof(TInterface), new[] { Assembly.GetExecutingAssembly() }).ToList();

            container.RegisterCollection<TInterface>(types);
            types.Iter(container.Register);
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