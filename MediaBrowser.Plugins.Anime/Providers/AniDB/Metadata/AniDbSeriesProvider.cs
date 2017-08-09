using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.Configuration;
using MediaBrowser.Plugins.Anime.Providers.AniDB.Converter;
using MediaBrowser.Plugins.Anime.Providers.AniDB.Identity;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Metadata
{
    public class AniDbSeriesProvider //: IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private const string SeriesDataFile = "series.xml";

        // AniDB has very low request rate limits, a minimum of 2 seconds between requests, and an average of 4 seconds between requests
        public static readonly SemaphoreSlim ResourcePool = new SemaphoreSlim(1, 1);

        public static readonly RateLimiter RequestLimiter =
            new RateLimiter(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));

        private static readonly int[] IgnoredCategoryIds =
            { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

        private static readonly Regex AniDbUrlRegex = new Regex(@"http://anidb.net/\w+ \[(?<name>[^\]]*)\]");
        private readonly AnidbConverter _anidbConverter;
        private readonly IApplicationPaths _appPaths;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;

        private readonly Dictionary<string, string> _typeMappings = new Dictionary<string, string>
        {
            { "Direction", PersonType.Director },
            { "Music", PersonType.Composer },
            { "Chief Animation Direction", "Chief Animation Director" }
        };

        public AniDbSeriesProvider(IApplicationPaths appPaths, IHttpClient httpClient, ILogManager logManager)
        {
            _appPaths = appPaths;
            _httpClient = httpClient;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
            _anidbConverter = new AnidbConverter(appPaths, logManager);

            TitleMatcher = AniDbTitleMatcher.DefaultInstance;
        }

        private IAniDbTitleMatcher TitleMatcher { get; }

        public int Order => -1;

        public string Name => "AniDB";

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();

            var anidbId = info.ProviderIds.GetOrDefault(ProviderNames.AniDb);
            if (string.IsNullOrEmpty(anidbId))
            {
                anidbId = await TitleMatcher.FindSeries(info.Name, cancellationToken).ConfigureAwait(false);
                _log.Debug($"{nameof(GetMetadata)}: AniDb series search result '{anidbId}'");
            }
            else
            {
                _log.Debug($"{nameof(GetMetadata)}: AniDb identity already set");
            }

            if (!string.IsNullOrEmpty(anidbId))
            {
                var tvDbId = GetTvDbSeriesId(anidbId);

                result.Item = new Series();
                result.HasMetadata = true;

                result.Item.ProviderIds.Add(ProviderNames.AniDb, anidbId);
                result.Item.SetProviderId(MetadataProviders.Tvdb, tvDbId);

                var seriesDataPath = await GetSeriesData(_appPaths, _httpClient, anidbId, cancellationToken);
                FetchSeriesInfo(result, seriesDataPath, info.MetadataLanguage ?? "en");

                _log.Debug(
                    $"{nameof(GetMetadata)}: Found metadata '{result.Item.Name}' AniDb id '{anidbId}' Tvdb id '{tvDbId}'");
            }

            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            var metadata = await GetMetadata(searchInfo, cancellationToken).ConfigureAwait(false);

            var list = new List<RemoteSearchResult>();

            if (metadata.HasMetadata)
            {
                var res = new RemoteSearchResult
                {
                    Name = metadata.Item.Name,
                    PremiereDate = metadata.Item.PremiereDate,
                    ProductionYear = metadata.Item.ProductionYear,
                    ProviderIds = metadata.Item.ProviderIds,
                    SearchProviderName = Name
                };

                list.Add(res);
            }

            return list;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private string GetTvDbSeriesId(string anidbSeriesId)
        {
            return _anidbConverter.Mapper.GetTvDbSeriesId(anidbSeriesId);
        }

        public static async Task<string> GetSeriesData(IApplicationPaths appPaths, IHttpClient httpClient,
            string seriesId, CancellationToken cancellationToken)
        {
            var dataPath = CalculateSeriesDataPath(appPaths, seriesId);
            var seriesDataPath = Path.Combine(dataPath, SeriesDataFile);
            var fileInfo = new FileInfo(seriesDataPath);

            // download series data if not present, or out of date
            if (!fileInfo.Exists || DateTime.UtcNow - fileInfo.LastWriteTimeUtc > TimeSpan.FromDays(7))
            {
                //await  DownloadSeriesData(seriesId, seriesDataPath, appPaths.CachePath, httpClient, cancellationToken)
                //    .ConfigureAwait(false);
            }

            return seriesDataPath;
        }

        public static string CalculateSeriesDataPath(IApplicationPaths paths, string seriesId)
        {
            return Path.Combine(paths.CachePath, "anidb", "series", seriesId);
        }

        private void FetchSeriesInfo(MetadataResult<Series> result, string seriesDataPath,
            string preferredMetadataLangauge)
        {
            var series = result.Item;
            var settings = new XmlReaderSettings
            {
                CheckCharacters = false,
                IgnoreProcessingInstructions = true,
                IgnoreComments = true,
                ValidationType = ValidationType.None
            };

            using (var streamReader = File.Open(seriesDataPath, FileMode.Open, FileAccess.Read))
            using (var reader = XmlReader.Create(streamReader, settings))
            {
                reader.MoveToContent();

                while (reader.Read())
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "startdate":
                                var val = reader.ReadElementContentAsString();

                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    DateTime date;
                                    if (DateTime.TryParse(val, CultureInfo.InvariantCulture,
                                        DateTimeStyles.AdjustToUniversal, out date))
                                    {
                                        date = date.ToUniversalTime();
                                        series.PremiereDate = date;
                                    }
                                }

                                break;
                            case "enddate":
                                var endDate = reader.ReadElementContentAsString();

                                if (!string.IsNullOrWhiteSpace(endDate))
                                {
                                    DateTime date;
                                    if (DateTime.TryParse(endDate, CultureInfo.InvariantCulture,
                                        DateTimeStyles.AdjustToUniversal, out date))
                                    {
                                        date = date.ToUniversalTime();
                                        series.EndDate = date;
                                    }
                                }

                                break;
                            case "titles":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    var title = ParseTitle(subtree, preferredMetadataLangauge);
                                    if (!string.IsNullOrEmpty(title))
                                    {
                                        series.Name = title;
                                    }
                                }

                                break;
                            case "creators":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseCreators(result, subtree);
                                }

                                break;
                            case "description":
                                series.Overview =
                                    ReplaceLineFeedWithNewLine(StripAniDbLinks(reader.ReadElementContentAsString()));

                                break;
                            case "ratings":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseRatings(series, subtree);
                                }

                                break;
                            case "resources":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseResources(series, subtree);
                                }

                                break;
                            case "characters":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseActors(result, subtree);
                                }

                                break;
                            case "tags":
                                using (var subtree = reader.ReadSubtree())
                                {
                                }

                                break;
                            case "categories":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseCategories(series, subtree);
                                }

                                break;
                            case "episodes":
                                using (var subtree = reader.ReadSubtree())
                                {
                                    ParseEpisodes(series, subtree);
                                }

                                break;
                        }
                    }
            }

            GenreHelper.CleanupGenres(series);
            GenreHelper.RemoveDuplicateTags(series);
        }

        private void ParseEpisodes(Series series, XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "episode")
                {
                    int id;
                    if (int.TryParse(reader.GetAttribute("id"), out id) && IgnoredCategoryIds.Contains(id))
                    {
                        continue;
                    }

                    using (var episodeSubtree = reader.ReadSubtree())
                    {
                        while (episodeSubtree.Read())
                            if (episodeSubtree.NodeType == XmlNodeType.Element)
                            {
                                switch (episodeSubtree.Name)
                                {
                                    case "epno":
                                        //var epno = episodeSubtree.ReadElementContentAsString();
                                        //EpisodeInfo info = new EpisodeInfo();
                                        //info.AnimeSeriesIndex = series.AnimeSeriesIndex;
                                        //info.IndexNumberEnd = string(epno);
                                        //info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb);
                                        //episodes.Add(info);
                                        break;
                                }
                            }
                    }
                }
        }

        private void ParseCategories(Series series, XmlReader reader)
        {
            var genres = new List<GenreInfo>();

            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "category")
                {
                    int weight;
                    if (!int.TryParse(reader.GetAttribute("weight"), out weight) || weight < 400)
                    {
                        continue;
                    }

                    int id;
                    if (int.TryParse(reader.GetAttribute("id"), out id) && IgnoredCategoryIds.Contains(id))
                    {
                        continue;
                    }

                    int parentId;
                    if (int.TryParse(reader.GetAttribute("parentid"), out parentId) &&
                        IgnoredCategoryIds.Contains(parentId))
                    {
                        continue;
                    }

                    using (var categorySubtree = reader.ReadSubtree())
                    {
                        while (categorySubtree.Read())
                            if (categorySubtree.NodeType == XmlNodeType.Element && categorySubtree.Name == "name")
                            {
                                var name = categorySubtree.ReadElementContentAsString();
                                genres.Add(new GenreInfo { Name = name, Weight = weight });
                            }
                    }
                }

            series.Genres = genres.OrderBy(g => g.Weight).Select(g => g.Name).ToList();
        }

        private void ParseResources(Series series, XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "resource")
                {
                    var type = reader.GetAttribute("type");

                    switch (type)
                    {
                        case "2":
                            var ids = new List<int>();

                            using (var idSubtree = reader.ReadSubtree())
                            {
                                while (idSubtree.Read())
                                    if (idSubtree.NodeType == XmlNodeType.Element && idSubtree.Name == "identifier")
                                    {
                                        int id;
                                        if (int.TryParse(idSubtree.ReadElementContentAsString(), out id))
                                        {
                                            ids.Add(id);
                                        }
                                    }
                            }

                            if (ids.Count > 0)
                            {
                                var firstId = ids.OrderBy(i => i).First().ToString(CultureInfo.InvariantCulture);
                                series.ProviderIds.Add(ProviderNames.MyAnimeList, firstId);
//                                series.ProviderIds.Add(ProviderNames.AniList, firstId);
                            }

                            break;
                        case "4":
                            while (reader.Read())
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "url")
                                {
                                    series.HomePageUrl = reader.ReadElementContentAsString();
                                    break;
                                }

                            break;
                    }
                }
        }

        private string StripAniDbLinks(string text)
        {
            return AniDbUrlRegex.Replace(text, "${name}");
        }

        public static string ReplaceLineFeedWithNewLine(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }

        private void ParseActors(MetadataResult<Series> series, XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "character")
                    {
                        using (var subtree = reader.ReadSubtree())
                        {
                            ParseActor(series, subtree);
                        }
                    }
                }
        }

        private void ParseActor(MetadataResult<Series> series, XmlReader reader)
        {
            string name = null;
            string role = null;

            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "name":
                            role = reader.ReadElementContentAsString();
                            break;
                        case "seiyuu":
                            name = reader.ReadElementContentAsString();
                            break;
                    }
                }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(role)
            ) // && series.People.All(p => p.Name != name))
            {
                series.AddPerson(CreatePerson(name, PersonType.Actor, role));
            }
        }

        private void ParseRatings(Series series, XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "permanent")
                    {
                        //int count;
                        //if (int.TryParse(reader.GetAttribute("count"), NumberStyles.Any, CultureInfo.InvariantCulture, out count))
                        //    series.VoteCount = count;

                        float rating;
                        if (float.TryParse(
                            reader.ReadElementContentAsString(),
                            NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture,
                            out rating))
                        {
                            series.CommunityRating = (float)Math.Round(rating, 1);
                        }
                    }
                }
        }

        private string ParseTitle(XmlReader reader, string preferredMetadataLangauge)
        {
            var titles = new List<Title>();

            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "title")
                {
                    var language = reader.GetAttribute("xml:lang");
                    var type = reader.GetAttribute("type");
                    var name = reader.ReadElementContentAsString();

                    titles.Add(new Title
                    {
                        Language = language,
                        Type = type,
                        Name = name
                    });
                }

            return titles.Localize(Plugin.Instance.Configuration.TitlePreference, preferredMetadataLangauge).Name;
        }

        private void ParseCreators(MetadataResult<Series> series, XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
                {
                    var type = reader.GetAttribute("type");
                    var name = reader.ReadElementContentAsString();

                    if (type == "Animation Work")
                    {
                        series.Item.Studios.Add(name);
                    }
                    else
                    {
                        series.AddPerson(CreatePerson(name, type));
                    }
                }
        }

        private PersonInfo CreatePerson(string name, string type, string role = null)
        {
            // todo find nationality of person and conditionally reverse name order

            string mappedType;
            if (!_typeMappings.TryGetValue(type, out mappedType))
            {
                mappedType = type;
            }

            return new PersonInfo
            {
                Name = ReverseNameOrder(name),
                Type = mappedType,
                Role = role
            };
        }

        public static string ReverseNameOrder(string name)
        {
            return name.Split(' ').Reverse().Aggregate(string.Empty, (n, part) => n + " " + part).Trim();
        }


        public static AniDbPersonInfo GetPersonInfo(string cachePath, string name)
        {
            var path = GetCastPath(name, cachePath);
            var serializer = new XmlSerializer(typeof(AniDbPersonInfo));

            try
            {
                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        return serializer.Deserialize(stream) as AniDbPersonInfo;
                    }
                }
            }
            catch (IOException)
            {
                return null;
            }

            return null;
        }

        private static string GetCastPath(string name, string cachePath)
        {
            name = name.ToLowerInvariant();
            return Path.Combine(cachePath, "anidb-people", name[0].ToString(), name + ".xml");
        }

        private static async Task SaveXml(string xml, string filename)
        {
            var writerSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Async = true
            };

            using (var writer = XmlWriter.Create(filename, writerSettings))
            {
                await writer.WriteRawAsync(xml).ConfigureAwait(false);
            }
        }

        private static async Task SaveEpsiodeXml(string seriesDataDirectory, string xml)
        {
            var episodeNumber = ParseEpisodeNumber(xml);

            if (episodeNumber != null)
            {
                var file = Path.Combine(seriesDataDirectory, string.Format("episode-{0}.xml", episodeNumber));
                await SaveXml(xml, file);
            }
        }

        private static string ParseEpisodeNumber(string xml)
        {
            var settings = new XmlReaderSettings
            {
                CheckCharacters = false,
                IgnoreProcessingInstructions = true,
                IgnoreComments = true,
                ValidationType = ValidationType.None
            };

            using (var streamReader = new StringReader(xml))
            {
                // Use XmlReader for best performance
                using (var reader = XmlReader.Create(streamReader, settings))
                {
                    reader.MoveToContent();

                    // Loop through each element
                    while (reader.Read())
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "epno")
                            {
                                var val = reader.ReadElementContentAsString();
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    return val;
                                }
                            }
                            else
                            {
                                reader.Skip();
                            }
                        }
                }
            }

            return null;
        }

        private struct GenreInfo
        {
            public string Name;
            public int Weight;
        }
    }

    public class Title
    {
        public string Language { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public static class TitleExtensions
    {
        public static Title Localize(this IEnumerable<Title> titles, TitlePreferenceType preference,
            string metadataLanguage)
        {
            var titlesList = titles as IList<Title> ?? titles.ToList();

            if (preference == TitlePreferenceType.Localized)
            {
                // prefer an official title, else look for a synonym
                var localized = titlesList.FirstOrDefault(t => t.Language == metadataLanguage && t.Type == "main") ??
                    titlesList.FirstOrDefault(t => t.Language == metadataLanguage && t.Type == "official") ??
                    titlesList.FirstOrDefault(t => t.Language == metadataLanguage && t.Type == "synonym");

                if (localized != null)
                {
                    return localized;
                }
            }

            if (preference == TitlePreferenceType.Japanese)
            {
                // prefer an official title, else look for a synonym
                var japanese = titlesList.FirstOrDefault(t => t.Language == "ja" && t.Type == "main") ??
                    titlesList.FirstOrDefault(t => t.Language == "ja" && t.Type == "official") ??
                    titlesList.FirstOrDefault(t => t.Language == "ja" && t.Type == "synonym");

                if (japanese != null)
                {
                    return japanese;
                }
            }

            // return the main title (romaji)
            return titlesList.FirstOrDefault(t => t.Language == "x-jat" && t.Type == "main") ??
                titlesList.FirstOrDefault(t => t.Type == "main") ??
                titlesList.FirstOrDefault();
        }

        /// <summary>
        ///     Gets the series data path.
        /// </summary>
        /// <param name="appPaths">The app paths.</param>
        /// <param name="seriesId">The series id.</param>
        /// <returns>System.String.</returns>
        internal static string GetSeriesDataPath(IApplicationPaths appPaths, string seriesId)
        {
            var seriesDataPath = Path.Combine(GetSeriesDataPath(appPaths), seriesId);

            return seriesDataPath;
        }

        /// <summary>
        ///     Gets the series data path.
        /// </summary>
        /// <param name="appPaths">The app paths.</param>
        /// <returns>System.String.</returns>
        internal static string GetSeriesDataPath(IApplicationPaths appPaths)
        {
            var dataPath = Path.Combine(appPaths.CachePath, "tvdb");

            return dataPath;
        }
    }
}