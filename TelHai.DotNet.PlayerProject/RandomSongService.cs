using System;
using System.Collections.Generic;

namespace TelHai.DotNet.PlayerProject
{
    public class RandomSongService : ISongService
    {
        private static RandomSongService? _instance;
        private static readonly object _lock = new object();
        private readonly Random _rnd = new Random();

        // Singleton instance property
        public static RandomSongService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new RandomSongService();
                }
            }
        }

        //  Private singleton constructor 
        private RandomSongService() { }

        // Banks of names with my fav artists
        private readonly string[] artistNames =
        {
            "Arctic Monkeys", "Eminem", "Coldplay", "Adele", "Drake",
            "The Weeknd", "Imagine Dragons", "Metallica", "Linkin Park", "Billie Eilish"
        };

        private readonly string[] songTitles =
        {
            "Sunrise", "Lost in Time", "Falling Stars", "Midnight Drive", "Broken Dreams",
            "Skyfall", "Thunderstorm", "Remember Me", "Gravity", "Blue Lights"
        };

        // 
        public List<MusicTrack> GenerateSongs(int count)
        {
            List<MusicTrack> result = new List<MusicTrack>();

            for (int i = 0; i < count; i++)
            {
                string artist = artistNames[_rnd.Next(artistNames.Length)];
                string title = songTitles[_rnd.Next(songTitles.Length)];

                double duration = Math.Round(_rnd.NextDouble() * 8 + 2, 1);
                // NextDouble gives 0.0–1.0 → multiply by 8 to get 0–8 → add 2 → 2–10 → round to 1 decimal

                MusicTrack song = new MusicTrack
                {
                    Artist = artist,
                    Title = title,
                    Duration = duration,
                    FilePath = "" // because these are generated, no MP3 file
                };

                result.Add(song);
            }

            return result;
        }
    }
}
