using System;
using System.Windows;

namespace TelHai.DotNet.PlayerProject
{
    public partial class AddManualSongWindow : Window
    {
        public string Artist { get; private set; } = "";
        public string Title { get; private set; } = "";
        public double Duration { get; private set; }

        public AddManualSongWindow()
        {
            InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtArtist.Text) ||
                string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtDuration.Text))
            {
                MessageBox.Show("All fields must be filled.");
                return;
            }

            if (!double.TryParse(txtDuration.Text, out double duration))
            {
                MessageBox.Show("Duration must be a number.");
                return;
            }

            if (duration < 1 || duration > 10)
            {
                MessageBox.Show("Duration must be between 1 and 10 minutes.");
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
