using System.Collections.Generic;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Data;

namespace MediaBrowser.Plugins.Anime.TvDb.Requests
{
    internal class GetEpisodesRequest : GetRequest<GetEpisodesRequest.Response>
    {
        public GetEpisodesRequest(int seriesId, int pageIndex) : base($"series/{seriesId}/episodes?page={pageIndex}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<TvDbEpisodeData> data, PageLinks links)
            {
                Data = data;
                Links = links;
            }

            public IEnumerable<TvDbEpisodeData> Data { get; }

            public PageLinks Links { get; }
        }

        public class PageLinks
        {
            public PageLinks(int first, int last, Maybe<int> next, Maybe<int> previous)
            {
                First = first;
                Last = last;
                Next = next;
                Previous = previous;
            }

            public int First { get; }

            public int Last { get; }

            public Maybe<int> Next { get; }

            public Maybe<int> Previous { get; }
        }
    }
}