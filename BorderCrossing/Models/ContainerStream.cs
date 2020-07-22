using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace BorderCrossing.Models
{
    public class ContainerStream : Stream
    {
        private readonly DeflateStream _stream;
        private int _lastProgress = 0;

        public ContainerStream(DeflateStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            _stream = stream;
        }

        protected DeflateStream ContainedStream { get { return _stream; } }

        public override bool CanRead { get { return _stream.CanRead; } }

        public override bool CanSeek { get { return _stream.CanSeek; } }

        public override bool CanWrite { get { return _stream.CanWrite; } }

        public override void Flush() { _stream.Flush(); }

        public override long Length { get { return _stream.Length; } }

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = _stream.Read(buffer, offset, count);
            if (ProgressChanged != null)
            {
                int newProgress = (int)(_stream.BaseStream.Position * 100.0 / _stream.BaseStream.Length);
                if (newProgress > _lastProgress)
                {
                    _lastProgress = newProgress;
                    ProgressChanged(this, new ProgressChangedEventArgs(_lastProgress, null));
                }
            }
            return amountRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        public event ProgressChangedEventHandler ProgressChanged;
    }
}
