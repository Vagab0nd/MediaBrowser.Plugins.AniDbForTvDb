using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal interface ITvDbConnection
    {
        Task<RequestResult<TResponseData>> PostAsync<TResponseData>(PostRequest<TResponseData> request,
            Option<string> token);

        Task<RequestResult<TResponseData>> GetAsync<TResponseData>(GetRequest<TResponseData> request,
            Option<string> token);
    }
}