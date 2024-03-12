using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;

namespace WhoWantsToBeMillionaire
{
    class Sound
    {
        private static readonly List<WaveOut> sounds = new List<WaveOut>();
        private static readonly WaveOut background = new WaveOut();

        private static Stream streamBack;
        private static WaveFileReader readerBack;
        private static LoopStream loopStream;

        private static float Volume = 1f;

        static Sound()
        {
            background.Volume = Volume;
        }

        public static void Play(string soundName)
        {
            Stream stream = ResourceManager.GetStream(soundName, TypeResource.Sounds);
            WaveFileReader reader = new WaveFileReader(stream);
            WaveOut waveOut = new WaveOut();

            sounds.Add(waveOut);

            waveOut.PlaybackStopped += Dispose;
            waveOut.Volume = Volume;
            waveOut.Init(reader);
            waveOut.Play();

            void Dispose(object sender, EventArgs e)
            {
                sounds.Remove(waveOut);
                waveOut.PlaybackStopped -= Dispose;

                waveOut.Dispose();
                reader.Dispose();
                stream.Dispose();
            }
        }

        public static void PlayBackground(string soundName)
        {
            StopBackground();

            streamBack = ResourceManager.GetStream(soundName, TypeResource.Sounds);
            readerBack = new WaveFileReader(streamBack);
            loopStream = new LoopStream(readerBack);

            background.Init(loopStream);
            background.Play();
        }

        public static void StopPeek()
        {
            if (sounds.Count > 0)
                sounds[sounds.Count - 1].Stop();
        }

        public static void StopBackground()
        {
            background?.Stop();
            streamBack?.Dispose();
            readerBack?.Dispose();
            loopStream?.Dispose();
        }

        public static void StopAll()
        {
            StopBackground();
            sounds.ForEach(s => s.Stop());
        }

        public static void SetVolume(float value)
        {
            background.Volume = Volume = value;
            sounds.ForEach(x => x.Volume = value);
        }
    }
}
