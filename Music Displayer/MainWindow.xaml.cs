using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Music_Displayer
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int FindMusicDelaySeconds = 5;

        private const string TrackFileName = "CurrentTrack.txt";

        private MusicSource m_musicSource;

        private Process m_currentProcess;

        private DispatcherTimer m_TrackCheckTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            LoadAvailableTrackSources();
            m_TrackCheckTimer.Tick += OnTrackCheckTimerTick;
            m_TrackCheckTimer.Interval = new TimeSpan(0, 0, FindMusicDelaySeconds);
            m_TrackCheckTimer.Start();
        }

        /// <summary>
        /// Find Spotify Track
        /// </summary>
        /// <returns></returns>
        private string FindSpotifyTrack()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.ProcessName.Contains("Spotify"))
                {
                    string songName = process.MainWindowTitle;
                    LogTrack(songName);
                    return songName;
                }
            }
            return "";
        }

        /// <summary>
        /// Find VLC track
        /// </summary>
        /// <returns>Song name</returns>
        private string FindVLCTrack()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.ProcessName.Contains("vlc"))
                {
                    string songName = process.MainWindowTitle;
                    RemoveIfContains(ref songName, " - Lettore multimediale VLC");
                    RemoveIfContains(ref songName, " - VLC media player");
                    RemoveIfContains(ref songName, " - Lecteur multimédia VLC");
                    LogTrack(songName);
                    return songName;
                }
            }
            return "";
        }

        /// <summary>
        /// Find youtube tracks
        /// </summary>
        /// <returns>Song name</returns>
        private string FindYoutubeTrack()
        {
            List<string> windows = new List<string>();
            WindowsHelper.GetAllWindows().Select(new Func<IntPtr, string>(WindowsHelper.GetTitle)).Where(x => x.Contains("- YouTube ")).ToList().ForEach(new Action<string>(windows.Add));
            foreach (string windowName in windows)
            {
                string songName = windowName;
                RemoveIfContains(ref songName, "- Google Chrome");
                RemoveIfContains(ref songName, "- Mozilla Firefox");
                RemoveIfContains(ref songName, "- Microsoft Edge");
                RemoveIfContains(ref songName, "- Opera");
                RemoveIfContains(ref songName, " - YouTube");
                LogTrack(songName);
                return songName;
            }
            return "";
        }

        /// <summary>
        /// Log the current track to a file
        /// </summary>
        /// <param name="name">Track Name</param>
        private void LogTrack(string name)
        {
            File.WriteAllText(TrackFileName, string.Empty);
            using (StreamWriter writer = File.Exists(TrackFileName) ? File.AppendText(TrackFileName) : new StreamWriter(TrackFileName))
            {
                writer.WriteLine($" {name}");
                writer.Close();
            }
        }

        /// <summary>
        /// Load all available tracks
        /// </summary>
        private void LoadAvailableTrackSources()
        {
            foreach (MusicSource ms in Enum.GetValues(typeof(MusicSource)))
                CmbTrackPlayers.Items.Add(ms);
        }

        /// <summary>
        /// Remove the given value if the given string contains it
        /// </summary>
        /// <param name="baseStr">Base string</param>
        /// <param name="value">Value</param>
        private void RemoveIfContains(ref string baseStr, string value)
        {
            if (baseStr.Contains(value))
                baseStr = baseStr.Trim().Replace(value, "");
        }

        private void OnTrackPlayerChanged(object sender, SelectionChangedEventArgs e)
        {
            m_musicSource = (MusicSource)CmbTrackPlayers.SelectedItem;
        }

        private void OnTrackCheckTimerTick(object sender, EventArgs e)
        {
            string trackName = null;
            switch (m_musicSource)
            {
                case MusicSource.Spofity:
                    trackName = FindSpotifyTrack();
                    break;
                case MusicSource.VLC:
                    trackName = FindVLCTrack();
                    break;
                case MusicSource.Youtube:
                    trackName = FindYoutubeTrack();
                    break;
            }
            lblTrack.Content = trackName;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogTrack("");
        }
    }
}
