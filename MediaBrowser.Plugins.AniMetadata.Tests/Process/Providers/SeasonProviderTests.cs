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
    public class SeasonProviderTests
    {
        [TestFixture]
        public class GetMetadata : SeasonProviderTests
        {
            [SetUp]
            public void Setup()
            {
                _seasonInfo = new SeasonInfo
                {
                    Name = "SeasonName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                _mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Season, "Failure reason"));

                _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                _mediaItemProcessor.GetResultAsync(_seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Returns(x => _mediaItemProcessorResult);

                _logManager = Substitute.For<ILogManager>();

                _logger = Substitute.For<ILogger>();
                _logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine($"Debug: {c.Arg<string>()}"));

                _logManager.GetLogger("SeasonProvider").Returns(_logger);

                _seasonProvider = new SeasonProvider(_logManager, _mediaItemProcessor);
            }

            private IMediaItemProcessor _mediaItemProcessor;
            private ILogManager _logManager;
            private ILogger _logger;
            private SeasonProvider _seasonProvider;
            private SeasonInfo _seasonInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Season>> _mediaItemProcessorResult;

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                _mediaItemProcessor.GetResultAsync(_seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(exception);

                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _logger.Received(1).ErrorException("Failed to get data for season 'SeasonName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                _mediaItemProcessor.GetResultAsync(_seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(new Exception("Failed"));

                var result = await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _seasonInfo.Name.Should().Be("SeasonName");
                _seasonInfo.IndexNumber.Should().Be(3);
                _seasonInfo.ParentIndexNumber.Should().Be(1);
                _seasonInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _logger.Received(1).Error("Failed to get data for season 'SeasonName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task ProvidesParentIds()
            {
                _seasonInfo.SeriesProviderIds = new Dictionary<string, string>
                {
                    { SourceNames.AniDb, "929" }
                };

                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _mediaItemProcessor.Received(1)
                    .GetResultAsync(_seasonInfo, MediaItemTypes.Season, Arg.Is<IEnumerable<EmbyItemId>>(ids => ids.Count() == 1 &&
                        ids.Single().Id == 929 &&
                        ids.Single().ItemType == MediaItemTypes.Series &&
                        ids.Single().SourceName == SourceNames.AniDb));
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _logger.Received(1).Info("Found data for season 'SeasonName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                _seasonInfo.Name.Should().BeEmpty();
                _seasonInfo.IndexNumber.Should().BeNull();
                _seasonInfo.ParentIndexNumber.Should().BeNull();
                _seasonInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await _seasonProvider.GetMetadata(_seasonInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}