using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TelHai.DotNet.PlayerProject.Services
{
    public class ITunesSearchService
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>
        /// Searches iTunes by term (song/artist/album).
        /// Returns a list of results (can be empty).
        /// Throws InvalidOperationException on network/parse errors with a user-friendly message.
        /// </summary>
        public async Task<List<ITunesTrackResult>> SearchAsync(string term, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<ITunesTrackResult>();

            // iTunes Search API:
            // entity=song -> returns tracks (songs)
            // limit -> max number of results
            // term -> query
            string url =
                "https://itunes.apple.com/search" +
                "?entity=song" +
                "&limit=25" +
                "&term=" + Uri.EscapeDataString(term.Trim());

            try
            {
                using HttpResponseMessage resp = await _http.GetAsync(url, cancellationToken);
                resp.EnsureSuccessStatusCode();

                string json = await resp.Content.ReadAsStringAsync(cancellationToken);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                ITunesSearchResponse? parsed = JsonSerializer.Deserialize<ITunesSearchResponse>(json, options);

                List<ITunesTrackResult> results = new List<ITunesTrackResult>();
                if (parsed?.Results == null)
                    return results;

                foreach (var r in parsed.Results)
                {
                    // Some fields may be null in API responses, guard with ?? ""
                    results.Add(new ITunesTrackResult
                    {
                        TrackName = r.TrackName ?? "",
                        ArtistName = r.ArtistName ?? "",
                        CollectionName = r.CollectionName ?? "",
                        ArtworkUrl100 = r.ArtworkUrl100 ?? "",
                        PreviewUrl = r.PreviewUrl ?? ""
                    });
                }

                return results;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected behavior; rethrow so UI can ignore it.
                throw;
            }
            catch (Exception ex)
            {
                // Wrap into a clean message for UI usage later
                throw new InvalidOperationException("Failed to search iTunes. Check internet connection and try again.", ex);
            }
        }

        /* ---------- JSON DTOs (internal) ---------- */

        private class ITunesSearchResponse
        {
            public int ResultCount { get; set; }
            public List<ITunesResultItem>? Results { get; set; }
        }

        private class ITunesResultItem
        {
            public string? TrackName { get; set; }
            public string? ArtistName { get; set; }
            public string? CollectionName { get; set; }
            public string? ArtworkUrl100 { get; set; }
            public string? PreviewUrl { get; set; }
        }
    }
}
