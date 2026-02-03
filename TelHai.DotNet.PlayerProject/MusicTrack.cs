using System;

namespace TelHai.DotNet.PlayerProject
{
    public class MusicTrack
    {
        public Guid Id { get; set; }

       
        public string Artist { get; set; } = string.Empty;

        
        public string Title { get; set; } = string.Empty;

        //  Duration in minutes double for 3.5 etc
        public double Duration { get; set; }

        // Extra: we still keep the file path for the player logic
        public string FilePath { get; set; } = string.Empty;

        // Constructor to automatically give each song a new GUID
        public MusicTrack()
        {
            Id = Guid.NewGuid();
        }

        // . ToString – how it will appears in the ListBox
        public override string ToString()
        {
            // Example: "Tamer Khatib – CSharp Song (3.5 min)"
            return $"{Artist} - {Title} ({Duration:F1} min)";
        }
    }
}
