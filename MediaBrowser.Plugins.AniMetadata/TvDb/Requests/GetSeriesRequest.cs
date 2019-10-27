using Emby.AniDbMetaStructure.JsonApi;
using Emby.AniDbMetaStructure.TvDb.Data;

namespace Emby.AniDbMetaStructure.TvDb.Requests
{
    internal class GetSeriesRequest : TvDbRequest<GetSeriesRequest.Response>, IGetRequest<GetSeriesRequest.Response>
    {
        public GetSeriesRequest(int tvDbSeriesId) : base($"series/{tvDbSeriesId}")
        {
        }

        public class Response
        {
            public Response(TvDbSeriesData data)
            {
                this.Data = data;
            }

            public TvDbSeriesData Data { get; }
        }
    }
}