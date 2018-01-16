using System;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NUnit.Framework;
using SimpleInjector;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class DependencyConfigurationTests
    {
        [Test]
        [TestCase(typeof(EpisodeProvider))]
        [TestCase(typeof(ImageProvider))]
        [TestCase(typeof(PersonImageProvider))]
        [TestCase(typeof(PersonProvider))]
        [TestCase(typeof(SeasonProvider))]
        [TestCase(typeof(SeriesProvider))]
        public void CanResolveEntryPoints(Type entryPointType)
        {
            Action action = () => DependencyConfiguration.Resolve(entryPointType, new TestApplicationHost());

            TestPlugin.EnsurePluginStaticSingletonAvailable();

            action.ShouldNotThrow();
        }

        [Test]
        public void ConfigurationIsValid()
        {
            DependencyConfiguration.Resolve<IRateLimiters>(new TestApplicationHost());

            TestPlugin.EnsurePluginStaticSingletonAvailable();

            DependencyConfiguration.Container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}