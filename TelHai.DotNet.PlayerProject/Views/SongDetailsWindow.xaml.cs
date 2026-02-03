using System.Windows;
using TelHai.DotNet.PlayerProject.ViewModels;

namespace TelHai.DotNet.PlayerProject.Views
{
    public partial class SongDetailsWindow : Window
    {
        public SongDetailsWindow(MusicTrack track)
        {
            InitializeComponent();
            DataContext = new SongDetailsViewModel(track);
        }
    }
}
