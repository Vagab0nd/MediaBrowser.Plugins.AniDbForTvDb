using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class EpisodeMatcherTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = Substitute.For<ILogManager>();
            _titleNormaliser = new TitleNormaliser();
        }

        private ILogManager _logManager;
        private ITitleNormaliser _titleNormaliser;

        [Test]
        public void FindEpisode_NoSeasonIndexProvided_MatchesOnTitle()
        {
            var episodes = new[]
            {
                new EpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new EpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new EpisodeMatcher(_titleNormaliser, _logManager);

            var foundEpisode =
                episodeMatcher.FindEpisode(episodes, Maybe<int>.Nothing, 3.ToMaybe(), "EpisodeTitle".ToMaybe());

            foundEpisode.Value.Should().Be(episodes[1]);
        }

        [Test]
        public void FindEpisode_NoTitleMatch_ReturnsNone()
        {
            var episodes = new[]
            {
                new EpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new EpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new EpisodeMatcher(_titleNormaliser, _logManager);

            var foundEpisode = episodeMatcher.FindEpisode(episodes, Maybe<int>.Nothing, 3.ToMaybe(), "Title".ToMaybe());

            foundEpisode.HasValue.Should().BeFalse();
        }

        [Test]
        public void FindEpisode_SeasonAndEpisodeIndexesProvided_MatchesOnIndexes()
        {
            var episodes = new[]
            {
                new EpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new EpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new EpisodeMatcher(_titleNormaliser, _logManager);

            var foundEpisode = episodeMatcher.FindEpisode(episodes, 1.ToMaybe(), 55.ToMaybe(), Maybe<string>.Nothing);

            foundEpisode.Value.Should().Be(episodes[1]);
        }
    }
}