using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class PropertyMappingTests
    {
        private class Source
        {
            public string SourceValue => "Source";
        }

        private class Target : BaseItem
        {
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public string TargetValue { get; set; } = "Target";
        }

        [Test]
        public void Apply_CopiesValueFromSourceToTarget()
        {
            var propertyMapping =
                PropertyMapping<Source, Target>.Create(t => t.TargetValue, (s, t) => t.TargetValue = s.SourceValue);

            var source = new Source();
            var target = new Target();

            propertyMapping.Apply(source, target);

            target.TargetValue.Should().Be("Source");
        }

        [Test]
        public void TargetPropertyName_ReturnsSelectedTargetProperty()
        {
            var propertyMapping =
                PropertyMapping<Source, Target>.Create(t => t.TargetValue, (s, t) => t.TargetValue = s.SourceValue);

            propertyMapping.TargetPropertyName.Should().Be("TargetValue");
        }
    }
}