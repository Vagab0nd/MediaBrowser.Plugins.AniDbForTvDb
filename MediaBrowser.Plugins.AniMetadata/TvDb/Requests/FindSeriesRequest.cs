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
            public MatchingSeries(int id)
            {
                this.Id = id;
            }

            public int Id { get; }
        }
    }
}