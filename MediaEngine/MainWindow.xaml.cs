using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using MediaEngine.GraphicsDisplay;

namespace MediaEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CueableMediaPlayer qplayer = new CueableMediaPlayer();
        Rectangle drect = new Rectangle();

        Rectangle prev = new Rectangle();

        public MainWindow()
        {
            InitializeComponent();

            qplayer.OnCuePresetStateChanged += Qplayer_OnCuePresetStateChanged;
            qplayer.OnVisualOutputChanged += Qplayer_OnVisualOutputChanged;
            qplayer.OnCurrentPlaybackPositionChanged += Qplayer_OnCurrentPlaybackPositionChanged;


            TestWindow fill_window = new TestWindow();
            fill_window.grid_main.Children.Add(qplayer);

            TestWindow preview_window = new TestWindow();
            preview_window.grid_main.Children.Add(prev);
            preview_window.Show();


            fill_window.Show();

            grid_main.Children.Add(drect);

            Dispatcher.BeginInvoke(async () => await Preload());
        }

        private void Qplayer_OnCurrentPlaybackPositionChanged(TimeSpan? current, TimeSpan? remaining, TimeSpan? duration)
        {
            Dispatcher.Invoke(() =>
            {
                if (remaining.HasValue)
                {
                    tbtime.Text = remaining.Value.ToString("mm\\:ss\\.fff");
                }
                else
                {
                    tbtime.Text = "Loading...";
                }
            });
        }

        private void Qplayer_OnVisualOutputChanged(VisualBrush current, VisualBrush preset)
        {
            drect.Fill = current;
            prev.Fill = preset;
        }

        private void Qplayer_OnCuePresetStateChanged(ICueableMediaPlayer.CueStatus status)
        {
            r_uncued.Visibility = status == ICueableMediaPlayer.CueStatus.Uncued ? Visibility.Visible : Visibility.Hidden;
            r_queuing.Visibility = status == ICueableMediaPlayer.CueStatus.Cueing ? Visibility.Visible : Visibility.Hidden;
            r_queued.Visibility = status == ICueableMediaPlayer.CueStatus.Cued ? Visibility.Visible : Visibility.Hidden;
        }

        private async Task Preload()
        {
            //files = Directory.GetFiles(@"D:\Main\church\livestream\2020\nov15\presentation").OrderBy(s => s).ToList();
            files = Directory.GetFiles(@"D:\hcav-onedrive\OneDrive - Holy Cross Lutheran Church\livestream-slides\2021-10-17\early_service\slides_auto_postset_premultiplied").ToList();

            await qplayer.CuePreset(new Uri(files[0]));
            await qplayer.SwapCurrentWithPreset();
            await qplayer.CuePreset(new Uri(files[1]));
            await qplayer.PlayCurrent();
        }



        int index = 2;
        List<string> files = new List<string>();
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                await Dispatcher.Invoke(() => qplayer.SwapCurrentWithPreset());
                //await qplayer.PlayCurrent();

                await Dispatcher.Invoke(() => qplayer.CuePreset(new Uri(files[index++])));
                if (index >= files.Count)
                {
                    index = 0;
                }
            }
            if (e.Key == Key.Left)
            {
                index--;
                if (index < 0)
                {
                    index = files.Count - 1;
                }
                qplayer.HotLoadCurrent(new Uri(files[index]));
                await qplayer.CuePreset(new Uri(files[index + 1]));
            }
            if (e.Key == Key.Up)
            {
                await qplayer.PlayCurrent();
            }
            if (e.Key == Key.Down)
            {
                await qplayer.PauseCurrent();
            }
            if (e.Key == Key.H)
            {
                qplayer.UseHighResPositionTimer = !qplayer.UseHighResPositionTimer;
            }
            if (e.Key == Key.OemPeriod)
            {
                await qplayer.PauseCurrent();
                await qplayer.AdvanceCurrent(TimeSpan.FromMilliseconds(16));
            }
            if (e.Key == Key.OemComma)
            {
                await qplayer.PauseCurrent();
                await qplayer.AdvanceCurrent(TimeSpan.FromMilliseconds(-16));
            }
        }
    }
}
