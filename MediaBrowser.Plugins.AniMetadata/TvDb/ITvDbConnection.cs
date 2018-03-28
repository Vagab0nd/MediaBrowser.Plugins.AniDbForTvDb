using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal interface ITvDbConnection
    {
        Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(PostRequest<TResponseData> request,
            Option<string> token);

        Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(GetRequest<TResponseData> request,
            Option<string> token);
    }
}