using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class MetadataMappingTests
    {
        private class AniDbSource
        {
            public string AniDbValue => "AniDb";
        }

        private class TvDbSource
        {
            public string TvDbValue => "TvDb";
        }

        private class Metadata : BaseItem
        {
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
            public string TargetValueA { get; set; } = "TargetValueA";

            public string TargetValueB { get; set; } = "TargetValueB";
        }

        [Test]
        public void Map_CopiesSourceDataToTarget()
        {
            var aniDbMapping =
                new PropertyMapping<AniDbSource, Metadata, string, string>(s => s.AniDbValue, t => t.TargetValueA);
            var tvDbMapping =
                new PropertyMapping<TvDbSource, Metadata, string, string>(s => s.TvDbValue, t => t.TargetValueB);

            var aniDbSource = new AniDbSource();
            var tvDbSource = new TvDbSource();
            var metadata = new Metadata();

            var metadataMapping = MetadataMapping<Metadata>.Create(new[] { aniDbMapping }, new[] { tvDbMapping });

            metadataMapping.Map(aniDbSource, tvDbSource, metadata);

            metadata.TargetValueA.Should().Be("AniDb");
            metadata.TargetValueB.Should().Be("TvDb");
        }
    }
}