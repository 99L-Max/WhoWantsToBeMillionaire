using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    static class GameSound
    {
        private static readonly List<WaveOut> s_sounds = new List<WaveOut>();

        private static float s_volume = 1f;

        public static void Play(string soundName, bool isTurnOff = true) =>
            Play(Resources.ResourceManager.GetStream(soundName), isTurnOff);

        public static void Play(UnmanagedMemoryStream stream, bool isTurnOff = true)
        {
            var reader = new WaveFileReader(stream);
            var waveOut = new WaveOut();

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

        public static void StopAll() =>
            s_sounds.ForEach(s => s.Stop());

        public static void SetVolume(float volume)
        {
            s_volume = volume;
            s_sounds.ForEach(x => x.Volume = volume);
        }
    }
}