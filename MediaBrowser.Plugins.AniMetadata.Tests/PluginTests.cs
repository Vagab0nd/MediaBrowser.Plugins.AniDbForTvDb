using System.Linq;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
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
                new TestPlugin().GetPages()
                    .Single()
                    .EmbeddedResourcePath;

            var assembly = typeof(Plugin).Assembly;

            assembly.GetManifestResourceNames().Should().Contain(embeddedResourcePath);
        }
    }
}