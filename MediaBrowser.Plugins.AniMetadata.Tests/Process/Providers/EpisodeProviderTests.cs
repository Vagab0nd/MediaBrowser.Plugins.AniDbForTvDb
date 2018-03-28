using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Providers;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Providers
{
    [TestFixture]
    public class EpisodeProviderTests
    {
        [TestFixture]
        public class GetMetadata : EpisodeProviderTests
        {
            [SetUp]
            public void Setup()
            {
                _episodeInfo = new EpisodeInfo
                {
                    Name = "EpisodeName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                _mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Episode, "Failure reason"));

                _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                _mediaItemProcessor.GetResultAsync(_episodeInfo, MediaItemTypes.Episode,
                        Arg.Any<IEnumerable<EmbyItemId>>())
                    .Returns(x => _mediaItemProcessorResult);

                _logManager = Substitute.For<ILogManager>();

                _logger = Substitute.For<ILogger>();
                _logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine($"Debug: {c.Arg<string>()}"));

                _logManager.GetLogger("EpisodeProvider").Returns(_logger);

                _episodeProvider = new EpisodeProvider(_logManager, _mediaItemProcessor);
            }

            private IMediaItemProcessor _mediaItemProcessor;
            private ILogManager _logManager;
            private ILogger _logger;
            private EpisodeProvider _episodeProvider;
            private EpisodeInfo _episodeInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Episode>> _mediaItemProcessorResult;

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                _mediaItemProcessor.GetResultAsync(_episodeInfo, MediaItemTypes.Episode, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(exception);

                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _logger.Received(1).ErrorException("Failed to get data for episode 'EpisodeName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                _mediaItemProcessor.GetResultAsync(_episodeInfo, MediaItemTypes.Episode, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(new Exception("Failed"));

                var result = await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _episodeInfo.Name.Should().Be("EpisodeName");
                _episodeInfo.IndexNumber.Should().Be(3);
                _episodeInfo.ParentIndexNumber.Should().Be(1);
                _episodeInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _logger.Received(1).Error("Failed to get data for episode 'EpisodeName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task ProvidesParentIds()
            {
                _episodeInfo.SeriesProviderIds = new Dictionary<string, string>
                {
                    { "AniDb", "929" }
                };

                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _mediaItemProcessor.Received(1)
                    .GetResultAsync(_episodeInfo, MediaItemTypes.Episode, Arg.Is<IEnumerable<EmbyItemId>>(ids => ids.Count() == 1 &&
                        ids.Single().Id == 929 &&
                        ids.Single().ItemType == MediaItemTypes.Series &&
                        ids.Single().SourceName == SourceNames.AniDb));
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _logger.Received(1).Info("Found data for episode 'EpisodeName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                _episodeInfo.Name.Should().BeEmpty();
                _episodeInfo.IndexNumber.Should().BeNull();
                _episodeInfo.ParentIndexNumber.Should().BeNull();
                _episodeInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await _episodeProvider.GetMetadata(_episodeInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}