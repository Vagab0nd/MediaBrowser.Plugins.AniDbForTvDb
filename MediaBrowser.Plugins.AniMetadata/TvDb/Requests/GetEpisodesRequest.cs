using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class GetEpisodesRequest : GetRequest<GetEpisodesRequest.Response>
    {
        public GetEpisodesRequest(int seriesId, int pageIndex) : base($"series/{seriesId}/episodes?page={pageIndex}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<TvDbEpisodeSummaryData> data, PageLinks links)
            {
                Data = data;
                Links = links;
            }

            public IEnumerable<TvDbEpisodeSummaryData> Data { get; }

            public PageLinks Links { get; }
        }

        public class PageLinks
        {
            public PageLinks(int first, int last, Option<int> next, Option<int> previous)
            {
                First = first;
                Last = last;
                Next = next;
                Previous = previous;
            }

            public int First { get; }

            public int Last { get; }

            public Option<int> Next { get; }

            public Option<int> Previous { get; }
        }
    }
}