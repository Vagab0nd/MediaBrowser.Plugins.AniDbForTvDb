using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListCharacterData
    {
        private readonly InnerCharacterData _node;

        public AniListCharacterData(InnerCharacterData node, IEnumerable<VoiceActorData> voiceActors)
        {
            _node = node;
            VoiceActors = voiceActors;
        }

        public AniListPersonNameData Name => _node.Name;

        public AniListImageUrlData Image => _node.Image;

        public IEnumerable<VoiceActorData> VoiceActors { get; }

        public class InnerCharacterData
        {
            public InnerCharacterData(AniListPersonNameData name, AniListImageUrlData image)
            {
                Name = name;
                Image = image;
            }

            public AniListPersonNameData Name { get; }
            public AniListImageUrlData Image { get; }
        }

        public class VoiceActorData
        {
            public VoiceActorData(string language, AniListImageUrlData image, AniListPersonNameData name)
            {
                Language = language;
                Image = image;
                Name = name;
            }

            public string Language { get; }
            public AniListImageUrlData Image { get; }
            public AniListPersonNameData Name { get; }
        }
    }
}