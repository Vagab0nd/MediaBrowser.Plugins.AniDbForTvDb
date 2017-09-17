using System.Threading.Tasks;
using MediaBrowser.Controller.Providers;
using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public interface ISeriesDataLoader
    {
        Task<OneOf<SeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(SeriesInfo info);
    }
}