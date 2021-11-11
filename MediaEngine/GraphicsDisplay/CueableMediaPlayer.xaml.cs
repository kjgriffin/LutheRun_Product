using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using Unosquare.FFME.Common;

namespace MediaEngine.GraphicsDisplay
{
    /// <summary>
    /// Interaction logic for CueableMediaPlayer.xaml
    /// </summary>
    public partial class CueableMediaPlayer : UserControl //, ICueableMediaPlayer
    {

        private DispatcherTimer m_lowResTimer;
        private DispatcherTimer m_highResTime;

        private VisualBrush brush_clipA;
        private VisualBrush brush_clipB;


        private Unosquare.FFME.MediaElement m_currentPlayer { get => m_currentSource == Bus.ClipA ? clip_a : clip_b; }
        private Unosquare.FFME.MediaElement m_presetPlayer { get => m_presetSource == Bus.ClipA ? clip_a : clip_b; }

        private Bus m_currentSource { get; set; }
        private Bus m_presetSource
        {
            get
            {
                return m_currentSource != Bus.ClipA ? Bus.ClipA : Bus.ClipB;
            }
        }


        enum Bus
        {
            ClipA,
            ClipB,
        }

        public CueableMediaPlayer()
        {
            InitializeComponent();
            m_currentSource = Bus.ClipA;
            brush_clipA = new VisualBrush(clip_a);
            brush_clipB = new VisualBrush(clip_b);
            clip_a.MediaOpened += Clip_a_MediaOpened;
            clip_a.MediaReady += Clip_a_MediaReady;
            clip_a.MediaEnded += Clip_a_MediaEnded;
            clip_b.MediaOpened += Clip_b_MediaOpened;
            clip_b.MediaReady += Clip_b_MediaReady;
            clip_b.MediaEnded += Clip_b_MediaEnded;

            m_lowResTimer = new DispatcherTimer();
            m_lowResTimer.Interval = TimeSpan.FromMilliseconds(500);
            m_lowResTimer.Tick += M_lowResTimer_Tick;
            m_lowResTimer.Start();

            m_highResTime = new DispatcherTimer();
            m_highResTime.Interval = TimeSpan.FromMilliseconds(16);
            m_highResTime.Tick += M_highResTime_Tick;
            //m_highResTime.Start();
        }

        private void M_highResTime_Tick(object sender, EventArgs e)
        {
            if (m_currentPlayer.IsPlaying)
            {
                OnCurrentPlaybackPositionChanged?.Invoke(m_currentPlayer?.Position, m_currentPlayer?.RemainingDuration, m_currentPlayer?.NaturalDuration);
            }
        }

        private void M_lowResTimer_Tick(object sender, EventArgs e)
        {
            if (m_currentPlayer.IsPlaying)
            {
                OnCurrentPlaybackPositionChanged?.Invoke(m_currentPlayer?.Position, m_currentPlayer?.RemainingDuration, m_currentPlayer?.NaturalDuration);
            }
        }

        private void Clip_b_MediaEnded(object sender, EventArgs e)
        {
            if (EnableLoopingPlayback && m_currentSource == Bus.ClipB)
            {
                clip_b.Seek(TimeSpan.FromMilliseconds(0));
                clip_b.Play();
            }
        }

        private void Clip_b_MediaReady(object sender, EventArgs e)
        {
            if (m_presetSource == Bus.ClipB)
            {
                OnCuePresetStateChanged?.Invoke(ICueableMediaPlayer.CueStatus.Cued);
            }
        }

        private void Clip_b_MediaOpened(object sender, MediaOpenedEventArgs e)
        {
            if (m_presetSource == Bus.ClipB)
            {
                OnCuePresetStateChanged?.Invoke(ICueableMediaPlayer.CueStatus.Cueing);
            }
        }

        private void Clip_a_MediaEnded(object sender, EventArgs e)
        {
            if (EnableLoopingPlayback && m_currentSource == Bus.ClipA)
            {
                clip_a.Seek(TimeSpan.FromMilliseconds(0));
                clip_a.Play();
            }
        }

        private void Clip_a_MediaReady(object sender, EventArgs e)
        {
            if (m_presetSource == Bus.ClipA)
            {
                OnCuePresetStateChanged?.Invoke(ICueableMediaPlayer.CueStatus.Cued);
            }
        }

        private void Clip_a_MediaOpened(object sender, MediaOpenedEventArgs e)
        {
            if (m_presetSource == Bus.ClipA)
            {
                OnCuePresetStateChanged?.Invoke(ICueableMediaPlayer.CueStatus.Cueing);
            }
        }

        public VisualBrush CurrentOutput { get => m_currentSource == Bus.ClipA ? brush_clipA : brush_clipB; }
        public VisualBrush PresetOutput { get => m_presetSource == Bus.ClipA ? brush_clipA : brush_clipB; }
        public bool EnableLoopingPlayback { get; set; }
        private double __m_audioLevel = 1;
        private double audioLevel
        {
            get
            {
                return __m_audioLevel;
            }
            set
            {
                __m_audioLevel = value;
                m_presetPlayer.Volume = value;
                m_currentPlayer.Volume = value;
            }
        }

        private bool __useHighResTimers = false;
        public bool UseHighResPositionTimer
        {
            get => __useHighResTimers;
            set
            {
                __useHighResTimers = value;
                if (value)
                {
                    m_lowResTimer.Stop();
                    m_highResTime.Start();
                }
                else
                {
                    m_highResTime.Stop();
                    m_lowResTimer.Start();
                }
            }
        }

        public event ICueableMediaPlayer.VisualOutputChanged OnVisualOutputChanged;
        public event ICueableMediaPlayer.CueStateUpdateArgs OnCuePresetStateChanged;
        public event ICueableMediaPlayer.MediaPlaybackPositionUpdateArgs OnCurrentPlaybackPositionChanged;

        public void HotLoadCurrent(Uri source)
        {
            Dispatcher.BeginInvoke(() =>
            {
                m_currentPlayer.Open(source);
            });
        }

        public async Task CuePreset(Uri source)
        {
            await Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await Dispatcher.Invoke(() => m_presetPlayer.Open(source));
                }
                catch (Exception)
                {
                    Debugger.Break();
                    throw;
                }
            });
        }

        public void MuteCurrent()
        {
            audioLevel = 0;
        }

        public async Task PauseCurrent()
        {
            await m_currentPlayer.Pause();
        }

        public async Task PlayCurrent()
        {
            await Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await Dispatcher.Invoke(() => m_currentPlayer.Play());
                }
                catch (Exception)
                {
                    Debugger.Break();
                    throw;
                }
            });
        }

        public async Task RestartCurrent()
        {
            await m_currentPlayer.Seek(TimeSpan.FromMilliseconds(0));
            await m_currentPlayer.Play();
        }

        public void SetCurrentAudioLevel(double level = 1)
        {
            audioLevel = level;
        }

        public async Task StopCurrent()
        {
            await m_currentPlayer.Stop();
        }

        public async Task SwapCurrentWithPreset()
        {
            await Dispatcher.BeginInvoke(async () =>
            {
                m_lowResTimer.Stop();
                m_highResTime.Stop();
                m_currentSource = m_currentSource != Bus.ClipA ? Bus.ClipA : Bus.ClipB;
                rect_display.Fill = CurrentOutput;
                if (m_presetPlayer.CanPause)
                {
                    await Dispatcher.Invoke(() => m_presetPlayer.Pause());
                }
            });
            OnVisualOutputChanged?.Invoke(CurrentOutput, PresetOutput);
        }

        public async Task SeekCurrent(TimeSpan time)
        {
            await m_currentPlayer.Seek(time);
            OnCurrentPlaybackPositionChanged?.Invoke(m_currentPlayer?.Position, m_currentPlayer?.RemainingDuration, m_currentPlayer?.NaturalDuration);
        }

        public async Task AdvanceCurrent(TimeSpan offset)
        {
            await m_currentPlayer.Seek(m_currentPlayer.Position.Add(offset));
            OnCurrentPlaybackPositionChanged?.Invoke(m_currentPlayer?.Position, m_currentPlayer?.RemainingDuration, m_currentPlayer?.NaturalDuration);
        }
    }
}
