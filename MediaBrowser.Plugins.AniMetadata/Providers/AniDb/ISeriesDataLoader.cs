using System.Threading.Tasks;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public interface ISeriesDataLoader
    {
        Task<SeriesData> GetSeriesDataAsync(string seriesName);
    }
}