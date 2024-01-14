using NAudio.Wave;
using System.IO;
using System.Reflection;

namespace WhoWantsToBeMillionaire
{
    static class Sound
    {
        private static WaveOut sound;
        private static WaveOut backgroundSound;

        private static void PlaySound(ref WaveOut waveOut, string soundName, bool isLoop)
        {
            waveOut?.Stop();
            waveOut?.Dispose();

            Stream stream = ResourceProcessing.GetStream(soundName + ".wav", TypeResource.Sounds);
            WaveFileReader reader = new WaveFileReader(stream);

            waveOut = new WaveOut();

            waveOut.PlaybackStopped += (s, e) =>
            {
                reader.Dispose();
                stream.Dispose();
            };

            waveOut.Init(reader);
            waveOut.Play();
        }

        public static void Play(string soundName)
        {
            PlaySound(ref sound, soundName, false);
        }

        public static void PlayBackground(string soundName)
        {
            PlaySound(ref backgroundSound, soundName, true);
        }

        public static void Stop()
        {
            sound?.Stop();
            sound?.Dispose();
        }

        public static void StopBackground()
        {
            backgroundSound?.Stop();
            backgroundSound?.Dispose();
        }
    }
}
