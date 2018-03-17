using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class EmbySeasonToTvDbTests
    {
        [SetUp]
        public void Setup()
        {
            var tvDbSource = Substitute.For<ITvDbSource>();

            _sources = Substitute.For<ISources>();
            _sources.TvDb.Returns(tvDbSource);

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Language.Returns("en");
        }

        private ISources _sources;
        private IEmbyItemData _embyItemData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new EmbySeasonToTvDb(_sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new EmbySeasonToTvDb(_sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new EmbySeasonToTvDb(_sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_ReturnsIdentifierOnlySourceData()
        {
            _embyItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));

            var loader = new EmbySeasonToTvDb(_sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().BeNull());
            result.IfRight(r => r.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Name")));
        }
    }
}