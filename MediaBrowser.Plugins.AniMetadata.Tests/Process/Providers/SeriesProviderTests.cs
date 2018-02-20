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
                SeriesInfo = new SeriesInfo
                {
                    Name = "SeriesName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                MediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Series, "Failure reason"));

                MediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                MediaItemProcessor.GetResultAsync(SeriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Returns(x => MediaItemProcessorResult);

                LogManager = Substitute.For<ILogManager>();

                Logger = Substitute.For<ILogger>();
                Logger.WhenForAnyArgs(l => l.Debug(null, null)).Do(c => Console.WriteLine($"Debug: {c.Arg<string>()}"));

                LogManager.GetLogger("SeriesProvider").Returns(Logger);

                PluginConfiguration = Substitute.For<IPluginConfiguration>();
                PluginConfiguration.ExcludedSeriesNames.Returns(Enumerable.Empty<string>());

                SeriesProvider = new SeriesProvider(LogManager, MediaItemProcessor, PluginConfiguration);
            }

            private IMediaItemProcessor MediaItemProcessor;
            private ILogManager LogManager;
            private ILogger Logger;
            private IPluginConfiguration PluginConfiguration;
            private SeriesProvider SeriesProvider;
            private SeriesInfo SeriesInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Series>> MediaItemProcessorResult;

            [Test]
            [TestCase("Exclude", "Exclude", true)]
            [TestCase("Exclude", "excLude", true)]
            [TestCase("Exclude", "Exclude1", false)]
            [TestCase("Exclude1", "Exclude", false)]
            public async Task EmbyTitleInExcludeList_LogsSkip(string name, string excludedName,
                bool isExcluded)
            {
                SeriesInfo.Name = name;

                PluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                if (isExcluded)
                {
                    Logger.Received(1).Info($"Skipping series '{name}' as it is excluded");
                }
                else
                {
                    Logger.DidNotReceive().Info($"Skipping series '{name}' as it is excluded");
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

                MediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                SeriesInfo.Name = name;

                PluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                var result = await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

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
                MediaItemProcessor.GetResultAsync(SeriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Throws(exception);

                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                Logger.Received(1).ErrorException("Failed to get data for series 'SeriesName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                MediaItemProcessor.GetResultAsync(SeriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>())
                    .Throws(new Exception("Failed"));

                var result = await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                SeriesInfo.Name.Should().Be("SeriesName");
                SeriesInfo.IndexNumber.Should().Be(3);
                SeriesInfo.ParentIndexNumber.Should().Be(1);
                SeriesInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                Logger.Received(1).Error("Failed to get data for series 'SeriesName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task GetsResult()
            {
                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                MediaItemProcessor.Received(1)
                    .GetResultAsync(SeriesInfo, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>());
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

                MediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                Logger.Received(1).Info("Found data for series 'SeriesName': 'MetadataName'");
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

                MediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                SeriesInfo.Name.Should().BeEmpty();
                SeriesInfo.IndexNumber.Should().BeNull();
                SeriesInfo.ParentIndexNumber.Should().BeNull();
                SeriesInfo.ProviderIds.Should().BeEmpty();
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

                MediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await SeriesProvider.GetMetadata(SeriesInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}