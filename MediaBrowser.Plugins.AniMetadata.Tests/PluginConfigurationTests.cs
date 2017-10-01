using System.Linq;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class PluginConfigurationTests
    {
        [Test]
        public void CanBeDeserialised()
        {
            var expected = new PluginConfiguration();

            var actual = new XmlSerialiser().Deserialise<PluginConfiguration>(
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<PluginConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <TitlePreference>Localized</TitlePreference>
  <MaxGenres>5</MaxGenres>
  <MoveExcessGenresToTags>true</MoveExcessGenresToTags>
  <AddAnimeGenre>true</AddAnimeGenre>
  <SeriesMappings>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Name</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>PremiereDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>EndDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>CommunityRating</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Overview</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Studios</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Genres</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Tags</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>People</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>AirDays</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>AirDays</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>AirDays</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>AirTime</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>AirTime</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>AirTime</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
  </SeriesMappings>
</PluginConfiguration>");

            actual.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public void CanBeSerialised()
        {
            var configuration = new PluginConfiguration();

            var serialised = new XmlSerialiser().Serialise(configuration);

            serialised.Should()
                .Be(@"<?xml version=""1.0"" encoding=""utf-16""?>
<PluginConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <TitlePreference>Localized</TitlePreference>
  <MaxGenres>5</MaxGenres>
  <MoveExcessGenresToTags>true</MoveExcessGenresToTags>
  <AddAnimeGenre>true</AddAnimeGenre>
  <SeriesMappings>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Name</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>PremiereDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>EndDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>CommunityRating</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Overview</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Studios</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Genres</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Tags</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>People</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>AirDays</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>AirDays</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>AirDays</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>AirTime</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>AirTime</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>AirTime</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
  </SeriesMappings>
  <SeasonMappings>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Name</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>PremiereDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>EndDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>CommunityRating</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Overview</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Studios</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Genres</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Tags</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
  </SeasonMappings>
  <EpisodeMappings>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Name</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>PremiereDate</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>RunTimeTicks</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>RunTimeTicks</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>RunTimeTicks</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>CommunityRating</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
    <PropertyMappingKeyCollection>
      <TargetPropertyName>Overview</TargetPropertyName>
      <Mappings>
        <PropertyMappingKey>
          <SourceName>AniDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>TvDB</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
        <PropertyMappingKey>
          <SourceName>None</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingKey>
      </Mappings>
    </PropertyMappingKeyCollection>
  </EpisodeMappings>
</PluginConfiguration>");
        }

        [Test]
        public void SeriesMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void SeriesMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().SeriesMappings.Concat(new[]
                {
                    new PropertyMappingKeyCollection("ExtraProperty",
                        new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Should().NotContain(m => m.TargetPropertyName == "ExtraProperty");
        }

        [Test]
        public void SeriesMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings[0].Mappings.ShouldBeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void SeriesMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings[0].Mappings.Last().ShouldBeEquivalentTo(missing);
        }

        [Test]
        public void SeriesMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Last().ShouldBeEquivalentTo(missing);
        }

        [Test]
        public void SeasonMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void SeasonMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().SeasonMappings.Concat(new[]
                {
                    new PropertyMappingKeyCollection("ExtraProperty",
                        new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Should().NotContain(m => m.TargetPropertyName == "ExtraProperty");
        }

        [Test]
        public void SeasonMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings[0].Mappings.ShouldBeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void SeasonMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings[0].Mappings.Last().ShouldBeEquivalentTo(missing);
        }

        [Test]
        public void SeasonMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Last().ShouldBeEquivalentTo(missing);
        }

        [Test]
        public void EpisodeMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void EpisodeMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().EpisodeMappings.Concat(new[]
                {
                    new PropertyMappingKeyCollection("ExtraProperty",
                        new[] { new PropertyMappingKey("ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Should().NotContain(m => m.TargetPropertyName == "ExtraProperty");
        }

        [Test]
        public void EpisodeMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings[0].Mappings.ShouldBeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void EpisodeMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings[0].Mappings.Last().ShouldBeEquivalentTo(missing);
        }

        [Test]
        public void EpisodeMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Last().ShouldBeEquivalentTo(missing);
        }
    }
}