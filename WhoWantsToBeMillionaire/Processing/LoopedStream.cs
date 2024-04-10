using NAudio.Wave;
using System;

namespace WhoWantsToBeMillionaire
{
    class LoopStream : WaveStream, IDisposable
    {
        private readonly WaveStream _sourceStream;

        public LoopStream(WaveStream sourceStream) =>
            _sourceStream = sourceStream;

        public override WaveFormat WaveFormat =>
            _sourceStream.WaveFormat;

        public override long Length =>
            _sourceStream.Length;

        public override long Position
        {
            set => _sourceStream.Position = value;
            get => _sourceStream.Position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0)
                        break;

                    _sourceStream.Position = 0;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _sourceStream.Dispose();

            base.Dispose(disposing);
        }
    }
}