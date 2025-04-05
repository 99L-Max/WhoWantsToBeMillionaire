using System.IO;
using NAudio.Wave;

namespace WhoWantsToBeMillionaire
{
    static class GameMusic
    {
        private static readonly WaveOut s_music = new WaveOut();

        private static float s_volume = 1f;
        private static Stream s_streamBack;
        private static WaveFileReader s_readerBack;
        private static LoopStream s_loopStream;

        public static bool IsPlaying { get; private set; } = false;

        public static void Play(UnmanagedMemoryStream stream)
        {
            Stop();

            s_streamBack = stream;
            s_readerBack = new WaveFileReader(s_streamBack);
            s_loopStream = new LoopStream(s_readerBack);

            s_music.Volume = s_volume;
            s_music.Init(s_loopStream);
            s_music.Play();

            IsPlaying = true;
        }

        public static void Stop()
        {
            s_music.Stop();
            s_streamBack?.Dispose();
            s_readerBack?.Dispose();
            s_loopStream?.Dispose();

            IsPlaying = false;
        }

        public static void SetVolume(float volume) =>
           s_music.Volume = s_volume = volume;
    }
}
