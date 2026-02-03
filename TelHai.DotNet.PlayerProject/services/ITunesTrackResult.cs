namespace TelHai.DotNet.PlayerProject.Services
{
    public class ITunesTrackResult
    {
        public string TrackName { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public string CollectionName { get; set; } = "";
        public string ArtworkUrl100 { get; set; } = "";
        public string PreviewUrl { get; set; } = "";

        public override string ToString()
        {
            // Useful when binding to a ListBox/ComboBox later
            return $"{ArtistName} - {TrackName}";
        }
    }
}
