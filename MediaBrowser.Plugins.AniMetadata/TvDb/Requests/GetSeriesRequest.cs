using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
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
                Data = data;
            }

            public TvDbSeriesData Data { get; }
        }
    }
}