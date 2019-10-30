using System.Collections.Generic;
using Emby.AniDbMetaStructure.JsonApi;

namespace Emby.AniDbMetaStructure.TvDb.Requests
{
    internal class FindSeriesRequest : TvDbRequest<FindSeriesRequest.Response>, IGetRequest<FindSeriesRequest.Response>
    {
        public FindSeriesRequest(string seriesName) : base($"search/series?name={seriesName}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<MatchingSeries> data)
            {
                this.MatchingSeries = data ?? new List<MatchingSeries>();
            }

            public IEnumerable<MatchingSeries> MatchingSeries { get; }
        }

        public class MatchingSeries
        {
            public MatchingSeries(int id, string seriesName, string[] aliases)
            {
                this.Id = id;
                this.SeriesName = seriesName;
                this.Aliases = aliases;
            }

            public int Id { get; }

            public string SeriesName { get; }

            public string[] Aliases { get; }
        }
    }
}