namespace JTLStudio.SDK.Editor.Toolkit.Data
{
    public class PlatformPresentation
    {
        public string DisplayName(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return "Yandex Games";

                case PlatformId.YouTubePlayables:
                    return "YouTube Playables";

                default:
                    return "Editor";
            }
        }

        public string PortalMark(PlatformId platform)
        {
            return platform == PlatformId.YouTubePlayables ? "youtube" : "yandex";
        }

        public string DescriptionKey(PlatformId platform)
        {
            switch (platform)
            {
                case PlatformId.YandexGames:
                    return "platform.yandexDescription";

                case PlatformId.YouTubePlayables:
                    return "platform.youtubeDescription";

                default:
                    return "platform.editorDescription";
            }
        }
    }
}
