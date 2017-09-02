using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime
{
    public class Plugin
        : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(
            applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "Anime";

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "anime",
                    EmbeddedResourcePath = "MediaBrowser.Plugins.Anime.Configuration.configPage.html"
                }
            };
        }
    }
}