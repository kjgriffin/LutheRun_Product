using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MediaEngine.GraphicsDisplay
{
    public interface ICueableMediaPlayer
    {
        public delegate void VisualOutputChanged(VisualBrush current, VisualBrush preset);
        public delegate void MediaPlaybackPositionUpdateArgs(TimeSpan? current, TimeSpan? remaining, TimeSpan? duration);
        public delegate void CueStateUpdateArgs(CueStatus status);

        public event VisualOutputChanged OnVisualOutputChanged;
        public event CueStateUpdateArgs OnCuePresetStateChanged;
        public event MediaPlaybackPositionUpdateArgs OnCurrentPlaybackPositionChanged;

        public VisualBrush CurrentOutput { get; }
        public VisualBrush PresetOutput { get; }

        public bool EnableLoopingPlayback { get; set; }

        public Task CuePreset(Uri source);
        public void SwapCurrentWithPreset();

        public Task PlayCurrent();
        public Task SeekCurrent(TimeSpan time);
        public Task AdvanceCurrent(TimeSpan offset);
        public Task PauseCurrent();
        public Task StopCurrent();
        public Task RestartCurrent();

        public void MuteCurrent();
        public void SetCurrentAudioLevel(double level = 1);


        public enum CueStatus
        {
            Uncued,
            Cued,
            Cueing,
        }
    }
}
