using System;
using System.Windows;

namespace TelHai.DotNet.PlayerProject
{
    public partial class AddSongWindow : Window
    {
        public string Artist { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public double Duration { get; private set; }

        public AddSongWindow(string defaultTitle)
        {
            InitializeComponent();
            txtTitle.Text = defaultTitle; // fill title automatically
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtDuration.Text, out double duration))
            {
                MessageBox.Show("Duration must be a number.");
                return;
            }

            Artist = txtArtist.Text;
            Title = txtTitle.Text;
            Duration = duration;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
