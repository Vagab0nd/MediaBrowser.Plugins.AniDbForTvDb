using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xem.Api;
using Xem.Api.Mapping;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal class XemTvDbMappingsFileSpec : IRemoteFileSpec<IDictionary<string, string[]>>, ICustomDownload<IDictionary<string, string[]>>
    {
        private readonly string rootPath;
        private readonly IApiClient xemApiClient;
        private readonly ICustomJsonSerialiser jsonSerialiser;

        public XemTvDbMappingsFileSpec(string rootPath, IApiClient xemApiClient, ICustomJsonSerialiser jsonSerialiser)
        {
            this.rootPath = rootPath;
            this.xemApiClient = xemApiClient;
            this.jsonSerialiser = jsonSerialiser;
        }

        public string Url => string.Empty;

        public string LocalPath => Path.Combine(this.rootPath, "xem-tvdb-anime-list.json");

        public bool IsGZipped => false;

        public ISerialiser Serialiser => this.jsonSerialiser;

        public async Task<string> DownloadFileAsync(IRemoteFileSpec<IDictionary<string, string[]>> fileSpec, CancellationToken cancellationToken)
        {
            var mappingsFromTvDb = await this.xemApiClient.GetAllNames(new AllNamesQuery(EntityType.TvDb));

            return JsonConvert.SerializeObject(mappingsFromTvDb.NameValues);
        }
    }
}
