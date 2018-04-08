namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListStudioData
    {
        private readonly StudioName _node;

        public AniListStudioData(bool isMain, StudioName node)
        {
            IsMain = isMain;
            _node = node;
        }

        public bool IsMain { get; }

        public string Name => _node.Name;

        public class StudioName
        {
            public StudioName(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}