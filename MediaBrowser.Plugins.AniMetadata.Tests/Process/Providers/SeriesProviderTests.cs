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
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Providers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Providers
{
    [TestFixture]
    public class SeriesProviderTests
    {
        [TestFixture]
        public class GetMetadata : SeriesProviderTests
        {
            [SetUp]
            public void Setup()
            {
                _seriesInfo = new SeriesInfo
                {
                    Name = "SeriesName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                _mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Series, "Failure reason"));

                _mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                _mediaItemProcessor.GetResultAsync(_seriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Returns(x => _mediaItemProcessorResult);

                _logManager = Substitute.For<ILogManager>();

                _logger = Substitute.For<ILogger>();
                _logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine($"Debug: {c.Arg<string>()}"));

                _logManager.GetLogger("SeriesProvider").Returns(_logger);

                _pluginConfiguration = Substitute.For<IPluginConfiguration>();
                _pluginConfiguration.ExcludedSeriesNames.Returns(Enumerable.Empty<string>());

                _seriesProvider = new SeriesProvider(_logManager, _mediaItemProcessor, _pluginConfiguration);
            }

            private IMediaItemProcessor _mediaItemProcessor;
            private ILogManager _logManager;
            private ILogger _logger;
            private IPluginConfiguration _pluginConfiguration;
            private SeriesProvider _seriesProvider;
            private SeriesInfo _seriesInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Series>> _mediaItemProcessorResult;

            [Test]
            [TestCase("Exclude", "Exclude", true)]
            [TestCase("Exclude", "excLude", true)]
            [TestCase("Exclude", "Exclude1", false)]
            [TestCase("Exclude1", "Exclude", false)]
            public async Task EmbyTitleInExcludeList_LogsSkip(string name, string excludedName,
                bool isExcluded)
            {
                _seriesInfo.Name = name;

                _pluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                if (isExcluded)
                {
                    _logger.Received(1).Info($"Skipping series '{name}' as it is excluded");
                }
                else
                {
                    _logger.DidNotReceive().Info($"Skipping series '{name}' as it is excluded");
                }
            }

            [Test]
            [TestCase("Exclude", "Exclude", true)]
            [TestCase("Exclude", "excLude", true)]
            [TestCase("Exclude", "Exclude1", false)]
            [TestCase("Exclude1", "Exclude", false)]
            public async Task EmbyTitleInExcludeList_ReturnsNoMetadata(string name, string excludedName,
                bool isExcluded)
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    },
                    HasMetadata = true
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                _seriesInfo.Name = name;

                _pluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                var result = await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                if (isExcluded)
                {
                    result.HasMetadata.Should().BeFalse();
                }
                else
                {
                    result.HasMetadata.Should().BeTrue();
                }
            }

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                _mediaItemProcessor.GetResultAsync(_seriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Throws(exception);

                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _logger.Received(1).ErrorException("Failed to get data for series 'SeriesName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                _mediaItemProcessor.GetResultAsync(_seriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Throws(new Exception("Failed"));

                var result = await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _seriesInfo.Name.Should().Be("SeriesName");
                _seriesInfo.IndexNumber.Should().Be(3);
                _seriesInfo.ParentIndexNumber.Should().Be(1);
                _seriesInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _logger.Received(1).Error("Failed to get data for series 'SeriesName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task GetsResult()
            {
                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _mediaItemProcessor.Received(1)
                    .GetResultAsync(_seriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>());
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _logger.Received(1).Info("Found data for series 'SeriesName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                _seriesInfo.Name.Should().BeEmpty();
                _seriesInfo.IndexNumber.Should().BeNull();
                _seriesInfo.ParentIndexNumber.Should().BeNull();
                _seriesInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                _mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await _seriesProvider.GetMetadata(_seriesInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}