using System.Collections.Generic;
using FluentAssertions;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
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

            dataCache.TitleList.Returns(new List<TitleListItem>
            {
                new TitleListItem
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitle
                        {
                            Title = "Test - ComparableMatch"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, logManager);

            var foundTitle = seriesTitleCache.FindSeriesByTitle($"Test{replacedCharacter} ComparableMatch");

            foundTitle.HasValue.Should().BeTrue();
            foundTitle.Value.AniDbId.Should().Be(123);
        }

        [Test]
        public void FindSeriesByTitle_YearSuffix_ReturnsCorrectSeries()
        {
            var dataCache = Substitute.For<IAniDbDataCache>();
            var logManager = Substitute.For<ILogManager>();

            dataCache.TitleList.Returns(new List<TitleListItem>
            {
                new TitleListItem
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitle
                        {
                            Title = "Bakuman."
                        }
                    }
                },
                new TitleListItem
                {
                    AniDbId = 456,
                    Titles = new[]
                    {
                        new ItemTitle
                        {
                            Title = "Bakuman. (2012)"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, logManager);

            var foundTitle = seriesTitleCache.FindSeriesByTitle("Bakuman (2012)");

            foundTitle.HasValue.Should().BeTrue();
            foundTitle.Value.AniDbId.Should().Be(456);
        }
    }
}