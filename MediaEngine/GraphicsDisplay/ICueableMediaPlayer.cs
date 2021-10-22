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
        public delegate void MediaPlaybackPositionUpdateArgs(TimeSpan current, TimeSpan duration);
        public delegate void CueStateUpdateArgs(CueStatus status);

        public event VisualOutputChanged OnVisualOutputChanged;
        public event CueStateUpdateArgs OnCuePresetStateChanged;
        public event MediaPlaybackPositionUpdateArgs OnCurrentPlaybackPositionChanged;

        public VisualBrush CurrentOutput { get; }
        public VisualBrush PresetOutput { get; }

        public bool EnableLoopingPlayback { get; set; }

        public void CuePreset(Uri source);
        public void SwapCurrentWithPreset();

        public void PlayCurrent();
        public void PauseCurrent();
        public void StopCurrent();
        public void RestartCurrent();

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
