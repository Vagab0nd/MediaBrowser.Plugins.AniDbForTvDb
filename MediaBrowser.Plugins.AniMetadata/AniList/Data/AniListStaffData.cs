namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListStaffData
    {
        private readonly InnerStaffData _node;

        public AniListStaffData(InnerStaffData node, string role)
        {
            _node = node;
            Role = role;
        }

        public AniListPersonNameData Name => _node.Name;

        public AniListImageUrlData Image => _node.Image;

        public string Role { get; }

        public class InnerStaffData
        {
            public InnerStaffData(AniListPersonNameData name, AniListImageUrlData image)
            {
                Name = name;
                Image = image;
            }

            public AniListPersonNameData Name { get; }

            public AniListImageUrlData Image { get; }
        }
    }
}