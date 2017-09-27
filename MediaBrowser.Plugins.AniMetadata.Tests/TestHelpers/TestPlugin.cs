using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using NSubstitute;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class TestPlugin : Plugin
    {
        public TestPlugin() : base(GetApplicationPaths(), Substitute.For<IXmlSerializer>())
        {
            Configuration = new PluginConfiguration();
        }

        public override string ConfigurationFileName => "FileName";

        public static void EnsurePluginStaticSingletonAvailable()
        {
            var x = new TestPlugin();
        }

        private static IApplicationPaths GetApplicationPaths()
        {
            var applicationPaths = Substitute.For<IApplicationPaths>();

            applicationPaths.PluginConfigurationsPath.Returns("Path");

            return applicationPaths;
        }
    }
}