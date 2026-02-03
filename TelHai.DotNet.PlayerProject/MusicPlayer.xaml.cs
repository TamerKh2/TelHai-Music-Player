using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TelHai.DotNet.PlayerProject;
using System.Linq;
using System;

namespace TelHai.DotNet.PlayerProject
{
    public partial class MusicPlayer : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";

        public MusicPlayer()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            this.Loaded += MusicPlayer_Loaded;

            mediaPlayer.Volume = sliderVolume.Value;
            LoadLibrary();
        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLibrary();
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
            txtStatus.Text = "Playing";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null &&
                mediaPlayer.NaturalDuration.HasTimeSpan &&
                !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    string defaultTitle = Path.GetFileNameWithoutExtension(file);

                    AddSongWindow details = new AddSongWindow(defaultTitle);
                    details.Owner = this;

                    if (details.ShowDialog() == true)
                    {
                        MusicTrack track = new MusicTrack
                        {
                            Artist = details.Artist,
                            Title = details.Title,
                            Duration = details.Duration,
                            FilePath = file
                        };

                        library.Add(track);
                    }
                }

                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is not MusicTrack track)
                return;

            if (string.IsNullOrWhiteSpace(track.FilePath))
            {
                MessageBox.Show("This song has no MP3 file attached (manual/random song).");
                return;
            }

            if (!File.Exists(track.FilePath))
            {
                MessageBox.Show("The MP3 file was not found on this computer.");
                return;
            }

            mediaPlayer.Open(new Uri(track.FilePath));
            mediaPlayer.Play();
            timer.Start();

            txtCurrentSong.Text = $"{track.Artist} - {track.Title} ({track.Duration:F1} min)";
            txtStatus.Text = "Playing";
        }

        private void SaveLibrary()
        {
            string json = JsonSerializer.Serialize(library);
            File.WriteAllText(FILE_NAME, json);
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        private void BtnLoad50_Click(object sender, RoutedEventArgs e)
        {
            ISongService service = RandomSongService.Instance;
            library = service.GenerateSongs(50);

            UpdateLibraryUI();
            SaveLibrary();
            txtStatus.Text = "Loaded 50 Random Songs";
        }

        private void BtnAddManual_Click(object sender, RoutedEventArgs e)
        {
            AddManualSongWindow win = new AddManualSongWindow();
            win.Owner = this;

            if (win.ShowDialog() == true)
            {
                MusicTrack newSong = new MusicTrack
                {
                    Artist = win.Artist,
                    Title = win.Title,
                    Duration = win.Duration,
                    FilePath = ""
                };

                library.Add(newSong);
                UpdateLibraryUI();
                SaveLibrary();
                txtStatus.Text = "Manual Song Added";
            }
        }

        /* -------------------- LINQ FEATURES -------------------- */

        private void BtnSortDurationAsc_Click(object sender, RoutedEventArgs e)
        {
            library = library.OrderBy(s => s.Duration).ToList();
            UpdateLibraryUI();
        }

        private void BtnSortDurationDesc_Click(object sender, RoutedEventArgs e)
        {
            library = library.OrderByDescending(s => s.Duration).ToList();
            UpdateLibraryUI();
        }

        private void BtnSortTitle_Click(object sender, RoutedEventArgs e)
        {
            library = library.OrderBy(s => s.Title).ToList();
            UpdateLibraryUI();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = txtSearch.Text.ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                UpdateLibraryUI();
                return;
            }

            var filtered = library.Where(s =>
                s.Title.ToLower().Contains(query) ||
                s.Artist.ToLower().Contains(query)
            ).ToList();

            lstLibrary.ItemsSource = filtered;
        }

        private void BtnShortSongs_Click(object sender, RoutedEventArgs e)
        {
            var shortSongs = library.Where(s => s.Duration < 3.0).ToList();
            lstLibrary.ItemsSource = shortSongs;
        }

        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            if (!library.Any())
            {
                MessageBox.Show("No songs to analyze.");
                return;
            }

            double total = library.Sum(s => s.Duration);
            double average = library.Average(s => s.Duration);
            var longest = library.OrderByDescending(s => s.Duration).First();

            MessageBox.Show(
                $"Total Duration: {total:F1} minutes\n" +
                $"Average Length: {average:F1} minutes\n" +
                $"Longest Song:\n{longest.Artist} - {longest.Title} ({longest.Duration:F1} min)"
            );
        }

        private void BtnGroup_Click(object sender, RoutedEventArgs e)
        {
            var groups = library
                .GroupBy(s => s.Artist)
                .Select(g => $"{g.Key}: {g.Count()} songs")
                .ToList();

            MessageBox.Show(string.Join("\n", groups), "Grouped by Artist");
        }

        // This method is not required but i added it bc it seems suitable here
        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete ALL songs?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                library.Clear();
                UpdateLibraryUI();
                SaveLibrary();
                txtStatus.Text = "Library cleared";
                txtCurrentSong.Text = "No Song Selected";
            }
        }
    }
}
