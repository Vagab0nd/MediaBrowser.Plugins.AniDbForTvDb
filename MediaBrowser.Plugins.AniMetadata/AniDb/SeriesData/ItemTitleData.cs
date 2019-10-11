using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    public class ItemTitleData
    {
        [XmlAttribute("xml:lang")]
        public string Language { get; set; }

        [XmlAttribute("type")]
        public virtual string Type { get; set; }

        [XmlText]
        public string Title { get; set; }

        public int Priority
        {
            get
            {
                switch (Type)
                {
                    case "main":
                        return 1;

                    case "official":
                        return 2;

                    case "syn":
                        return 3;

                    default:
                        return 4;
                }
            }
        }

        public override string ToString()
        {
            return $"{{Lang: {Language}, Type: {Type}, Title: {Title}}}";
        }
    }
}