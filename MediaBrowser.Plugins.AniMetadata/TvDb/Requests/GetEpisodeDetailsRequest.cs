using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class GetEpisodeDetailsRequest : GetRequest<GetEpisodeDetailsRequest.Response>
    {
        public GetEpisodeDetailsRequest(int episodeId) : base($"episodes/{episodeId}")
        {
        }

        public class Response
        {
            public Response(TvDbEpisodeData data)
            {
                Data = data;
            }

            public TvDbEpisodeData Data { get; }
        }
    }
}