﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BorderCrossing.Models
{
    public abstract class ContainerStream : Stream
    {
        private Stream _stream;

        protected ContainerStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            _stream = stream;
        }

        protected Stream ContainedStream { get { return _stream; } }

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
            return _stream.Read(buffer, offset, count);
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
    }
}
