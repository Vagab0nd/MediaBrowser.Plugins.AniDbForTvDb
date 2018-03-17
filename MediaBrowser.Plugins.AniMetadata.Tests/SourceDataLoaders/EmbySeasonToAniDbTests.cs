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
    public class EmbySeasonToAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            var aniDbSource = Substitute.For<IAniDbSource>();

            _sources = Substitute.For<ISources>();
            _sources.AniDb.Returns(aniDbSource);

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Language.Returns("en");
        }

        private ISources _sources;
        private IEmbyItemData _embyItemData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new EmbySeasonToAniDb(_sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new EmbySeasonToAniDb(_sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new EmbySeasonToAniDb(_sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoIndex_SetsIndexToOne()
        {
            _embyItemData.Identifier.Returns(new ItemIdentifier(Option<int>.None, Option<int>.None, "Name"));

            var loader = new EmbySeasonToAniDb(_sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().BeNull());
            result.IfRight(r => r.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(1, Option<int>.None, "Name")));
        }

        [Test]
        public async Task LoadFrom_ReturnsIdentifierOnlySourceData()
        {
            _embyItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));

            var loader = new EmbySeasonToAniDb(_sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().BeNull());
            result.IfRight(r => r.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Name")));
        }
    }
}