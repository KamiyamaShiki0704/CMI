using System;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace CMI
{
    internal static class LowLatencyMusicBackend
    {
        private static readonly object SyncRoot = new object();
        private static WasapiOut output;
        private static MediaFoundationReader reader;
        private static VolumeSampleProvider volumeProvider;
        private static string currentSong;
        private static bool loop;
        private static bool stopping;

        public static string CurrentSong
        {
            get
            {
                lock (SyncRoot)
                {
                    return currentSong;
                }
            }
        }

        public static bool Play(string songPath, bool shouldLoop, int volume)
        {
            lock (SyncRoot)
            {
                try
                {
                    if (string.Equals(currentSong, songPath, StringComparison.OrdinalIgnoreCase) && output != null)
                    {
                        SetVolumeNoLock(volume);
                        return true;
                    }

                    DisposeNoLock();
                    currentSong = songPath;
                    loop = shouldLoop;
                    stopping = false;
                    reader = new MediaFoundationReader(songPath);
                    volumeProvider = new VolumeSampleProvider(reader.ToSampleProvider());
                    SetVolumeNoLock(volume);
                    output = new WasapiOut(AudioClientShareMode.Shared, false, 20);
                    output.PlaybackStopped += OutputOnPlaybackStopped;
                    output.Init(volumeProvider);
                    output.Play();
                    return true;
                }
                catch
                {
                    DisposeNoLock();
                    return false;
                }
            }
        }

        public static void SetVolume(string songPath, int volume)
        {
            lock (SyncRoot)
            {
                if (!string.Equals(currentSong, songPath, StringComparison.OrdinalIgnoreCase)) return;
                SetVolumeNoLock(volume);
            }
        }

        public static void Stop(string songPath)
        {
            lock (SyncRoot)
            {
                if (!string.Equals(currentSong, songPath, StringComparison.OrdinalIgnoreCase)) return;
                stopping = true;
                try
                {
                    output?.Stop();
                }
                catch
                {
                    // Stop must never escape into the game-state timer.
                }
                DisposeNoLock();
            }
        }

        private static void OutputOnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            lock (SyncRoot)
            {
                if (!ReferenceEquals(sender, output)) return;
                if (!stopping && loop && reader != null && output != null)
                {
                    try
                    {
                        reader.Position = 0;
                        output.Play();
                        return;
                    }
                    catch
                    {
                        // Fall through and dispose the failed playback state.
                    }
                }
                DisposeNoLock();
            }
        }

        private static void SetVolumeNoLock(int volume)
        {
            if (volumeProvider == null) return;
            volumeProvider.Volume = Math.Max(0, Math.Min(100, volume)) / 100f;
        }

        private static void DisposeNoLock()
        {
            try
            {
                if (output != null)
                {
                    output.PlaybackStopped -= OutputOnPlaybackStopped;
                    output.Dispose();
                }
            }
            catch { }

            try
            {
                reader?.Dispose();
            }
            catch { }

            output = null;
            reader = null;
            volumeProvider = null;
            currentSong = null;
            loop = false;
            stopping = false;
        }
    }
}
