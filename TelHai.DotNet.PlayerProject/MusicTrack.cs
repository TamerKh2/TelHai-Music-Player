using System;
using System.Collections.Generic;

namespace TelHai.DotNet.PlayerProject
{
    public class MusicTrack
    {
        // Unique identifier (already used, keep as-is)
        public Guid Id { get; set; }

        // Basic song info
        public string Artist { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        // Duration in minutes (e.g. 3.5)
        public double Duration { get; set; }

        // Local MP3 path (empty for manual / random songs)
        public string FilePath { get; set; } = string.Empty;

        /* ---------- NEW FIELDS (for API + MVVM window) ---------- */

        // Album name (from iTunes API or manual edit)
        public string AlbumName { get; set; } = string.Empty;

        // iTunes preview URL (optional)
        public string PreviewUrl { get; set; } = string.Empty;

        // Album artwork URL (optional)
        public string ArtworkUrl { get; set; } = string.Empty;

        // User-managed images (local paths or URLs)
        public List<string> Images { get; set; } = new List<string>();

        // Constructor – auto-generate GUID
        public MusicTrack()
        {
            Id = Guid.NewGuid();
        }

        // How the song appears in the ListBox
        public override string ToString()
        {
            // Example: "Artist - Title (3.5 min)"
            return $"{Artist} - {Title} ({Duration:F1} min)";
        }
    }
}
