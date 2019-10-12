using System;
using System.Collections.Generic;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    using Infrastructure;

    [TestFixture]
    public class OptionJsonConverterTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
            this.dataNull = new TvDbEpisodeSummaryData(1, "Test", Option<long>.None, 1, 2, 2, new DateTime(2007, 10, 6),
                "Overview");
            this.dataNonNull = new TvDbEpisodeSummaryData(1, "Test", 5L, 1, 2, 2, new DateTime(2007, 10, 6), "Overview");
        }

        private TvDbEpisodeSummaryData dataNull;
        private TvDbEpisodeSummaryData dataNonNull;

        [Test]
        public void AirDayEnum_CanDeserialise()
        {
            var serialised = @"{
  string.EmptyIdstring.Empty: 78914,
  string.EmptySeriesNamestring.Empty: string.EmptyFull Metal Panic!string.Empty,
  string.EmptyAirsDayOfWeekstring.Empty: 1
}";

            JsonConvert.DeserializeObject<TvDbSeriesData>(serialised)
                .Should()
                .BeEquivalentTo(new TvDbSeriesData(78914, "Full Metal Panic!", Option<DateTime>.None, null, 0,
                    Option<AirDay>.Some(AirDay.Tuesday), null, 0, null, null, null));
        }

        [Test]
        public void ComplexMaybeSome_CanDeserialise()
        {
            var serialised = @"
{
  string.Emptylinksstring.Empty: {
    string.Emptyfirststring.Empty: 1,
    string.Emptylaststring.Empty: 1,
    string.Emptynextstring.Empty: null,
    string.Emptyprevstring.Empty: null
  },
  string.Emptydatastring.Empty: [
    {
      string.EmptyabsoluteNumberstring.Empty: 1,
      string.EmptyairedEpisodeNumberstring.Empty: 1,
      string.EmptyairedSeasonstring.Empty: 1,
      string.EmptyairedSeasonIDstring.Empty: 29182,
      string.EmptydvdEpisodeNumberstring.Empty: null,
      string.EmptydvdSeasonstring.Empty: null,
      string.EmptyepisodeNamestring.Empty: string.EmptyCelestial Beingstring.Empty,
      string.EmptyfirstAiredstring.Empty: string.Empty2007-10-06string.Empty,
      string.Emptyidstring.Empty: 340368,
      string.Emptylanguagestring.Empty: {
        string.EmptyepisodeNamestring.Empty: string.Emptyenstring.Empty,
        string.Emptyoverviewstring.Empty: string.Emptyenstring.Empty
      },
      string.EmptylastUpdatedstring.Empty: 1496255818,
      string.Emptyoverviewstring.Empty: string.EmptyCelestial Being, a private army dedicated to eradicating war, begins demonstrating the powers of their new \string.EmptyMS-GUNDAM\string.Empty suits by interrupting the public demonstration of AEU's latest Mobile Suit, the AEU Enact and by protecting the Human Reform League's Space Elevator, \string.EmptyTenchu\string.Empty from being attacked by terrorists when their mobile suits had attempted to launch rockets on the \string.EmptyTenchu\string.Empty, earning a news appearance from various TV news channels where Celestial Being's goals were publicly stated by Aeoria Schenberg.string.Empty
    }
  ]
}";

            JsonConvert.DeserializeObject<GetEpisodesRequest.Response>(serialised)
                .Should()
                .BeEquivalentTo(new GetEpisodesRequest.Response(
                        new[]
                        {
                            new TvDbEpisodeSummaryData(340368, "Celestial Being", 1L, 1, 1, 1496255818,
                                new DateTime(2007, 10, 6),
                                @"Celestial Being, a private army dedicated to eradicating war, begins demonstrating the powers of their new string.EmptyMS-GUNDAMstring.Empty suits by interrupting the public demonstration of AEU's latest Mobile Suit, the AEU Enact and by protecting the Human Reform League's Space Elevator, string.EmptyTenchustring.Empty from being attacked by terrorists when their mobile suits had attempted to launch rockets on the string.EmptyTenchustring.Empty, earning a news appearance from various TV news channels where Celestial Being's goals were publicly stated by Aeoria Schenberg.")
                        },
                        new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None)),
                    o => o.Excluding(i =>
                        i.SelectedMemberInfo.Name == "Value" &&
                        i.SelectedMemberInfo.DeclaringType == typeof(Option<int>)));
        }

        [Test]
        public void MaybeNone_CanDeserialise()
        {
            var serialised = @"{
		string.EmptyIdstring.Empty: 1,
		string.EmptyEpisodeNamestring.Empty: string.EmptyTeststring.Empty,
		string.EmptyAbsoluteNumberstring.Empty: null,
		string.EmptyAiredEpisodeNumberstring.Empty: 1,
		string.EmptyAiredSeasonstring.Empty: 2,
		string.EmptyLastUpdatedstring.Empty: 2,
        string.EmptyfirstAiredstring.Empty: string.Empty2007-10-06string.Empty,
        string.Emptyoverviewstring.Empty: string.EmptyOverviewstring.Empty
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeSummaryData>(serialised)
                .Should()
                .BeEquivalentTo(this.dataNull);
        }

        [Test]
        public void MaybeNone_CanSerialise()
        {
            JsonConvert.SerializeObject(this.dataNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":null,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2,\"FirstAired\":\"2007-10-06T00:00:00\",\"Overview\":\"Overview\"}");
        }

        [Test]
        public void MaybeSome_CanDeserialise()
        {
            var serialised = @"{
		string.EmptyIdstring.Empty: 1,
		string.EmptyEpisodeNamestring.Empty: string.EmptyTeststring.Empty,
		string.EmptyAbsoluteNumberstring.Empty: 5,
		string.EmptyAiredEpisodeNumberstring.Empty: 1,
		string.EmptyAiredSeasonstring.Empty: 2,
		string.EmptyLastUpdatedstring.Empty: 2,
        string.EmptyfirstAiredstring.Empty: string.Empty2007-10-06string.Empty,
        string.Emptyoverviewstring.Empty: string.EmptyOverviewstring.Empty
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeSummaryData>(serialised)
                .Should()
                .BeEquivalentTo(this.dataNonNull);
        }

        [Test]
        public void MaybeSome_CanSerialise()
        {
            JsonConvert.SerializeObject(this.dataNonNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":5,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2,\"FirstAired\":\"2007-10-06T00:00:00\",\"Overview\":\"Overview\"}");
        }
    }
}