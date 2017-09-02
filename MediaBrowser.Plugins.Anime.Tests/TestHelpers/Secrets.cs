using System;
using System.IO;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    public class Secrets
    {
        private static readonly Lazy<Secrets> InstanceLazy = new Lazy<Secrets>(() =>
            JsonConvert.DeserializeObject<Secrets>(
                File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/secrets.json")));

        public Secrets(string tvDbApiKey)
        {
            TvDbApiKey = tvDbApiKey;
        }

        public string TvDbApiKey { get; }

        public static Secrets Instance => InstanceLazy.Value;
    }
}