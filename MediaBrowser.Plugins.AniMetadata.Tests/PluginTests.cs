using System.Linq;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class PluginTests
    {
        [Test]
        public void ConfigurationPageExists()
        {
            var embeddedResourcePath =
                new Plugin(Substitute.For<IApplicationPaths>(), Substitute.For<IXmlSerializer>()).GetPages()
                    .Single()
                    .EmbeddedResourcePath;

            var assembly = typeof(Plugin).Assembly;

            assembly.GetManifestResourceNames().Should().Contain(embeddedResourcePath);
        }
    }
}