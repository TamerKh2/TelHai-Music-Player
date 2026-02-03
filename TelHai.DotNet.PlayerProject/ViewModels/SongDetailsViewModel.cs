using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
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

            HeaderText = $"Editing: {_track.Artist} - {_track.Title}";
            SearchTerm = $"{_track.Artist} {_track.Title}".Trim();

            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !IsBusy);
            CancelCommand = new RelayCommand(_ => CancelSearch(), _ => IsBusy);
        }

        public string HeaderText { get; }

        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
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
            }
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand CancelCommand { get; }

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
            catch (Exception ex)
            {
                StatusText = "Error searching iTunes.";
                // Keep message user-friendly; details are in ex if needed
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
    }
}
