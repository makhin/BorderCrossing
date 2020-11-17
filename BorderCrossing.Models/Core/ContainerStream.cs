using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace BorderCrossing.Models.Core
{
    public class ContainerStream : Stream
    {
        private int _lastProgress = 0;

        public ContainerStream(Stream stream)
        {
            ContainedStream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        protected Stream ContainedStream { get; }

        public override bool CanRead => ContainedStream.CanRead;

        public override bool CanSeek => ContainedStream.CanSeek;

        public override bool CanWrite => ContainedStream.CanWrite;

        public override void Flush() { ContainedStream.Flush(); }

        public override long Length => ContainedStream.Length;

        public override long Position
        {
            get => ContainedStream.Position;
            set => ContainedStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = ContainedStream.Read(buffer, offset, count);
            if (ProgressChanged == null)
            {
                return amountRead;
            }

            int newProgress;

            if (ContainedStream is DeflateStream deflateStream)
            {
                var stream = deflateStream.BaseStream;
                newProgress = (int)(stream.Position * 100.0 / stream.Length);
            }
            else
            {
                newProgress = (int)(ContainedStream.Position * 100.0 / ContainedStream.Length);
            }

            if (newProgress > _lastProgress)
            {
                _lastProgress = newProgress;
                ProgressChanged(this, new ProgressChangedEventArgs(_lastProgress, null));
            }
            return amountRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return ContainedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ContainedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ContainedStream.Write(buffer, offset, count);
        }

        public event ProgressChangedEventHandler ProgressChanged;
    }
}
