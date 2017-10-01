using System.Threading.Tasks;
using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public interface ISeriesDataLoader
    {
        Task<OneOf<SeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(string seriesName);
    }
}