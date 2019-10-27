using System.Collections.Generic;
using Emby.AniDbMetaStructure.JsonApi;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.TvDb.Requests
{
    internal class GetEpisodesRequest : TvDbRequest<GetEpisodesRequest.Response>, IGetRequest<GetEpisodesRequest.Response>
    {
        public GetEpisodesRequest(int seriesId, int pageIndex) : base($"series/{seriesId}/episodes?page={pageIndex}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<TvDbEpisodeSummaryData> data, PageLinks links)
            {
                this.Data = data;
                this.Links = links;
            }

            public IEnumerable<TvDbEpisodeSummaryData> Data { get; }

            public PageLinks Links { get; }
        }

        public class PageLinks
        {
            public PageLinks(int first, int last, Option<int> next, Option<int> previous)
            {
                this.First = first;
                this.Last = last;
                this.Next = next;
                this.Previous = previous;
            }

            public int First { get; }

            public int Last { get; }

            public Option<int> Next { get; }

            public Option<int> Previous { get; }
        }
    }
}