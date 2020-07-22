using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace BorderCrossing.Models
{
    public class ReadProgressStream : ContainerStream
    {
        private int _lastProgress = 0;

        public ReadProgressStream(DeflateStream stream) : base(stream)
        {
            if (!stream.CanRead) throw new ArgumentException("stream");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = base.Read(buffer, offset, count);
            if (ProgressChanged != null)
            {
                int newProgress = (int)(Position * 100.0 / Length);
                if (newProgress > _lastProgress)
                {
                    _lastProgress = newProgress;
                    ProgressChanged(this, new ProgressChangedEventArgs(_lastProgress, null));
                }
            }
            return amountRead;
        }

        public event ProgressChangedEventHandler ProgressChanged;
    }
}
