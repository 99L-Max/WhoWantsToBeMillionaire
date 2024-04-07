using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class Sound
    {
        private static readonly List<WaveOut> s_sounds = new List<WaveOut>();
        private static readonly WaveOut s_background = new WaveOut();

        private static float s_volume = 1f;
        private static Stream s_streamBack;
        private static WaveFileReader s_readerBack;
        private static LoopStream s_loopStream;

        static Sound() =>
            s_background.Volume = s_volume;

        public static void Play(string soundName, bool isTurnOff = true) =>
            Play(Resources.ResourceManager.GetStream(soundName), isTurnOff);

        public static void Play(UnmanagedMemoryStream stream, bool isTurnOff = true)
        {
            WaveFileReader reader = new WaveFileReader(stream);
            WaveOut waveOut = new WaveOut();

            if (isTurnOff)
                s_sounds.Add(waveOut);

            waveOut.PlaybackStopped += Dispose;
            waveOut.Volume = s_volume;
            waveOut.Init(reader);
            waveOut.Play();

            void Dispose(object sender, EventArgs e)
            {
                s_sounds.Remove(waveOut);
                waveOut.PlaybackStopped -= Dispose;

                waveOut.Dispose();
                reader.Dispose();
                stream.Dispose();
            }
        }

        public static void PlayBackground(UnmanagedMemoryStream stream)
        {
            StopBackground();

            s_streamBack = stream;
            s_readerBack = new WaveFileReader(s_streamBack);
            s_loopStream = new LoopStream(s_readerBack);

            s_background.Init(s_loopStream);
            s_background.Play();
        }

        public static void StopPeek()
        {
            if (s_sounds.Count > 0)
                s_sounds[s_sounds.Count - 1].Stop();
        }

        public static void StopBackground()
        {
            s_background?.Stop();
            s_streamBack?.Dispose();
            s_readerBack?.Dispose();
            s_loopStream?.Dispose();
        }

        public static void StopAll()
        {
            StopBackground();
            s_sounds.ForEach(s => s.Stop());
        }

        public static void SetVolume(float value)
        {
            s_background.Volume = s_volume = value;
            s_sounds.ForEach(x => x.Volume = value);
        }
    }
}
