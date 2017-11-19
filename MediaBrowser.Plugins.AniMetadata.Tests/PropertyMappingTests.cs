using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
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
                new PropertyMapping<Source, Target, string>("", t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    "");

            var source = new Source();
            var target = new Target();

            propertyMapping.Apply(source, target);

            target.TargetValue.Should().Be("Source");
        }

        [Test]
        public void CanApply_MatchingSourceAndTargetTypes_ReturnsTrue()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>("", t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    "");

            propertyMapping.CanApply(new Source(), new Target()).Should().BeTrue();
        }

        [Test]
        public void CanApply_MismatchingSourceType_ReturnsFalse()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>("", t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    "");

            propertyMapping.CanApply(new object(), new Target());
        }

        [Test]
        public void CanApply_MismatchingTargetType_ReturnsFalse()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>("", t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    "");

            propertyMapping.CanApply(new Source(), new object()).Should().BeFalse();
        }

        [Test]
        public void TargetPropertyName_ReturnsSelectedTargetProperty()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>("", t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    "");

            propertyMapping.TargetPropertyName.Should().Be("TargetValue");
        }
    }
}