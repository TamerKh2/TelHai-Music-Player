using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TelHai.DotNet.PlayerProject.Helpers;
using TelHai.DotNet.PlayerProject.Services;

namespace TelHai.DotNet.PlayerProject.ViewModels
{
    public class SongDetailsViewModel : BaseViewModel
    {
        private readonly MusicTrack _track;
        private readonly ITunesSearchService _service = new ITunesSearchService();
        private CancellationTokenSource? _cts;

        public SongDetailsViewModel(MusicTrack track)
        {
            _track = track;

            // init from track
            _trackArtist = _track.Artist;
            _trackTitle = _track.Title;
            _trackDurationText = _track.Duration.ToString("F1");

            _albumName = _track.AlbumName;
            _previewUrl = _track.PreviewUrl;
            _artworkUrl = _track.ArtworkUrl;

            RefreshArtworkImage();

            Images = new ObservableCollection<string>(_track.Images ?? new System.Collections.Generic.List<string>());

            SearchTerm = $"{_track.Artist} {_track.Title}".Trim();

            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => CancelSearch(), _ => IsBusy);

            ApplySelectedCommand = new RelayCommand(_ => ApplySelected(), _ => SelectedResult != null);
            OpenPreviewCommand = new RelayCommand(_ => OpenPreview(), _ => !string.IsNullOrWhiteSpace(PreviewUrl));

            AddImageCommand = new RelayCommand(_ => AddImage());
            RemoveImageCommand = new RelayCommand(_ => RemoveSelectedImage(), _ => !string.IsNullOrWhiteSpace(SelectedImage));

            SaveSongCommand = new RelayCommand(_ => SaveSongJson());
            LoadSongCommand = new RelayCommand(_ => LoadSongJson());
        }

        /* ---------- Header ---------- */
        public string HeaderText => $"Editing: {TrackArtist} - {TrackTitle}";

        private void NotifyHeader() => OnPropertyChanged(nameof(HeaderText));

        /* ---------- Editable Song Fields ---------- */

        private string _trackArtist = "";
        public string TrackArtist
        {
            get => _trackArtist;
            set
            {
                _trackArtist = value ?? "";
                _track.Artist = _trackArtist;
                OnPropertyChanged();
                NotifyHeader();
            }
        }

        private string _trackTitle = "";
        public string TrackTitle
        {
            get => _trackTitle;
            set
            {
                _trackTitle = value ?? "";
                _track.Title = _trackTitle;
                OnPropertyChanged();
                NotifyHeader();
            }
        }

        private string _trackDurationText = "0";
        public string TrackDurationText
        {
            get => _trackDurationText;
            set
            {
                _trackDurationText = value ?? "0";
                OnPropertyChanged();

                if (double.TryParse(_trackDurationText, out double d) && d >= 0)
                {
                    _track.Duration = d;
                    StatusText = "Duration updated.";
                }
                else
                {
                    StatusText = "Duration must be a valid number.";
                }
            }
        }

        /* ---------- Search ---------- */

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value ?? "";
                OnPropertyChanged();
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value ?? "";
                OnPropertyChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();

                (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ITunesTrackResult> Results { get; } = new ObservableCollection<ITunesTrackResult>();

        private ITunesTrackResult? _selectedResult;
        public ITunesTrackResult? SelectedResult
        {
            get => _selectedResult;
            set
            {
                _selectedResult = value;
                OnPropertyChanged();
                (ApplySelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand ApplySelectedCommand { get; }
        public RelayCommand OpenPreviewCommand { get; }

        private async Task SearchAsync()
        {
            CancelSearch();
            _cts = new CancellationTokenSource();

            Results.Clear();
            SelectedResult = null;

            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                StatusText = "Type something to search.";
                return;
            }

            try
            {
                IsBusy = true;
                StatusText = "Searching iTunes...";

                var items = await _service.SearchAsync(SearchTerm, _cts.Token);

                foreach (var item in items)
                    Results.Add(item);

                StatusText = items.Count == 0 ? "No results found." : $"Found {items.Count} results.";
            }
            catch (OperationCanceledException)
            {
                StatusText = "Search canceled.";
            }
            catch
            {
                StatusText = "Error searching iTunes.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CancelSearch()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
                _cts.Cancel();
        }

        /* ---------- Applied Metadata ---------- */

        private string _albumName = "";
        public string AlbumName
        {
            get => _albumName;
            set
            {
                _albumName = value ?? "";
                _track.AlbumName = _albumName;
                OnPropertyChanged();
            }
        }

        private string _previewUrl = "";
        public string PreviewUrl
        {
            get => _previewUrl;
            set
            {
                _previewUrl = value ?? "";
                _track.PreviewUrl = _previewUrl;
                OnPropertyChanged();
                (OpenPreviewCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _artworkUrl = "";
        public string ArtworkUrl
        {
            get => _artworkUrl;
            set
            {
                _artworkUrl = value ?? "";
                _track.ArtworkUrl = _artworkUrl;
                OnPropertyChanged();
                RefreshArtworkImage();
            }
        }

        private ImageSource? _artworkImage;
        public ImageSource? ArtworkImage
        {
            get => _artworkImage;
            set
            {
                _artworkImage = value;
                OnPropertyChanged();
            }
        }

        private void RefreshArtworkImage()
        {
            if (string.IsNullOrWhiteSpace(ArtworkUrl))
            {
                ArtworkImage = null;
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ArtworkUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                ArtworkImage = bitmap;
            }
            catch
            {
                ArtworkImage = null;
            }
        }

        private void ApplySelected()
        {
            if (SelectedResult == null)
                return;

            if (!string.IsNullOrWhiteSpace(SelectedResult.ArtistName))
                TrackArtist = SelectedResult.ArtistName;

            if (!string.IsNullOrWhiteSpace(SelectedResult.TrackName))
                TrackTitle = SelectedResult.TrackName;

            AlbumName = SelectedResult.CollectionName ?? "";
            PreviewUrl = SelectedResult.PreviewUrl ?? "";
            ArtworkUrl = SelectedResult.ArtworkUrl100 ?? "";

            StatusText = "Applied selected result to song.";
        }

        private void OpenPreview()
        {
            if (string.IsNullOrWhiteSpace(PreviewUrl))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = PreviewUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                StatusText = "Failed to open preview link.";
            }
        }

        /* ---------- Images Management ---------- */

        public ObservableCollection<string> Images { get; }

        private string? _selectedImage;
        public string? SelectedImage
        {
            get => _selectedImage;
            set
            {
                _selectedImage = value;
                OnPropertyChanged();
                (RemoveImageCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddImageCommand { get; }
        public RelayCommand RemoveImageCommand { get; }

        private void AddImage()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select Image",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                foreach (var path in ofd.FileNames)
                {
                    if (!Images.Contains(path))
                        Images.Add(path);
                }
                SyncImagesToTrack();
                StatusText = "Images updated.";
            }
        }

        private void RemoveSelectedImage()
        {
            if (string.IsNullOrWhiteSpace(SelectedImage))
                return;

            Images.Remove(SelectedImage);
            SelectedImage = null;

            SyncImagesToTrack();
            StatusText = "Image removed.";
        }

        private void SyncImagesToTrack()
        {
            _track.Images = new System.Collections.Generic.List<string>(Images);
        }

        /* ---------- Save/Load Song JSON ---------- */

        public RelayCommand SaveSongCommand { get; }
        public RelayCommand LoadSongCommand { get; }

        private void SaveSongJson()
        {
            try
            {
                var sfd = new SaveFileDialog
                {
                    Title = "Save Song JSON",
                    Filter = "JSON file|*.json",
                    FileName = "song.json"
                };

                if (sfd.ShowDialog() != true)
                    return;

                // Ensure track has latest images
                SyncImagesToTrack();

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_track, options);
                File.WriteAllText(sfd.FileName, json);

                StatusText = "Song saved to JSON.";
            }
            catch
            {
                StatusText = "Failed to save JSON.";
            }
        }

        private void LoadSongJson()
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Title = "Load Song JSON",
                    Filter = "JSON file|*.json"
                };

                if (ofd.ShowDialog() != true)
                    return;

                string json = File.ReadAllText(ofd.FileName);
                var loaded = JsonSerializer.Deserialize<MusicTrack>(json);

                if (loaded == null)
                {
                    StatusText = "Invalid JSON file.";
                    return;
                }

                // Copy into the existing _track (important: keep same reference used by library)
                ApplyLoadedSongToTrack(loaded);

                // Refresh UI fields
                TrackArtist = _track.Artist;
                TrackTitle = _track.Title;
                TrackDurationText = _track.Duration.ToString("F1");

                AlbumName = _track.AlbumName;
                PreviewUrl = _track.PreviewUrl;
                ArtworkUrl = _track.ArtworkUrl;

                Images.Clear();
                foreach (var img in _track.Images ?? new System.Collections.Generic.List<string>())
                    Images.Add(img);

                StatusText = "Song loaded from JSON.";
            }
            catch
            {
                StatusText = "Failed to load JSON.";
            }
        }

        private void ApplyLoadedSongToTrack(MusicTrack loaded)
        {
            // Keep Id and FilePath as you prefer:
            // - If you want to keep current FilePath, keep it.
            // - If JSON includes FilePath and you want to allow overwriting, change it.
            string keepFilePath = _track.FilePath;
            Guid keepId = _track.Id;

            _track.Artist = loaded.Artist ?? "";
            _track.Title = loaded.Title ?? "";
            _track.Duration = loaded.Duration;

            _track.AlbumName = loaded.AlbumName ?? "";
            _track.PreviewUrl = loaded.PreviewUrl ?? "";
            _track.ArtworkUrl = loaded.ArtworkUrl ?? "";

            _track.Images = loaded.Images ?? new System.Collections.Generic.List<string>();

            // Preserve identity/path
            _track.FilePath = keepFilePath;
            _track.Id = keepId;
        }
    }
}
