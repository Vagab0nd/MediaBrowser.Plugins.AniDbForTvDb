﻿using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using Emby.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeriesFromAniDbEpisodeTests
    {
        [SetUp]
        public void Setup()
        {
            this.aniDbSource = Substitute.For<IAniDbSource>();
            this.aniDbSource.Name.Returns(SourceNames.AniDb);

            this.sources = Substitute.For<ISources>();
            this.sources.AniDb.Returns(this.aniDbSource);

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            embyItemData.Language.Returns("en");

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(embyItemData);

            this.aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            this.aniDbSource.GetSeriesData(embyItemData, Arg.Any<ProcessResultContext>())
                .Returns(this.aniDbSeriesData);
        }

        private ISources sources;
        private AniDbSeriesData aniDbSeriesData;
        private IAniDbSource aniDbSource;
        private IMediaItem mediaItem;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var sourceData = Substitute.For<ISourceData<AniDbEpisodeData>>();

            var loader = new AniDbSeriesFromAniDbEpisode(this.sources);

            loader.CanLoadFrom(sourceData).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeriesFromAniDbEpisode(this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var sourceData = Substitute.For<ISourceData<AniDbSeriesData>>();

            var loader = new AniDbSeriesFromAniDbEpisode(this.sources);

            loader.CanLoadFrom(sourceData).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            this.sources.AniDb.SelectTitle(this.aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbSeriesFromAniDbEpisode(this.sources);

            var result = await loader.LoadFrom(this.mediaItem, null);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(this.aniDbSeriesData));
            result.IfRight(sd => sd.Source.Should().BeEquivalentTo(this.sources.AniDb.ForAdditionalData()));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            this.aniDbSource.GetSeriesData(this.mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, AniDbSeriesData>(new ProcessFailedResult(string.Empty, string.Empty,
                    MediaItemTypes.Series, "Failed to find series in AniDb")));

            var loader = new AniDbSeriesFromAniDbEpisode(this.sources);

            var result = await loader.LoadFrom(this.mediaItem, null);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in AniDb"));
        }
    }
}