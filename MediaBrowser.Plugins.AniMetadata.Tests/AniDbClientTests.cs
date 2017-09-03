using System.Collections.Generic;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class SeriesTitleCacheTests
    {
        [Test]
        [TestCase(@"/")]
        [TestCase(@",")]
        [TestCase(@".")]
        [TestCase(@":")]
        [TestCase(@";")]
        [TestCase(@"\")]
        [TestCase(@"(")]
        [TestCase(@")")]
        [TestCase(@"{")]
        [TestCase(@"}")]
        [TestCase(@"[")]
        [TestCase(@"]")]
        [TestCase(@"+")]
        [TestCase(@"-")]
        [TestCase(@"_")]
        [TestCase(@"=")]
        [TestCase(@"–")]
        [TestCase(@"*")]
        [TestCase(@"""")]
        [TestCase(@"'")]
        [TestCase(@"!")]
        [TestCase(@"`")]
        [TestCase(@"?")]
        public void FindSeriesByTitle_ComparableTitleMatch_ReturnsSeries(string replacedCharacter)
        {
            var dataCache = Substitute.For<IAniDbDataCache>();
            var logManager = Substitute.For<ILogManager>();

            dataCache.TitleList.Returns(new List<TitleListItemData>
            {
                new TitleListItemData
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Test - ComparableMatch"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, new TitleNormaliser(), logManager);

            var foundTitle = seriesTitleCache.FindSeriesByTitle($"Test{replacedCharacter} ComparableMatch");

            foundTitle.IsSome.Should().BeTrue();
            foundTitle.ValueUnsafe().AniDbId.Should().Be(123);
        }

        [Test]
        public void FindSeriesByTitle_YearSuffix_ReturnsCorrectSeries()
        {
            var dataCache = Substitute.For<IAniDbDataCache>();
            var logManager = Substitute.For<ILogManager>();

            dataCache.TitleList.Returns(new List<TitleListItemData>
            {
                new TitleListItemData
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Bakuman."
                        }
                    }
                },
                new TitleListItemData
                {
                    AniDbId = 456,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Bakuman. (2012)"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, new TitleNormaliser(), logManager);

            var foundTitle = seriesTitleCache.FindSeriesByTitle("Bakuman (2012)");

            foundTitle.IsSome.Should().BeTrue();
            foundTitle.ValueUnsafe().AniDbId.Should().Be(456);
        }
    }
}