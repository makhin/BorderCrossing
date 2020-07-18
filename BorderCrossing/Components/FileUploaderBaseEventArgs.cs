using System;
using System.IO;

namespace BorderCrossing.Components
{
    public class FileUploaderBaseEventArgs : EventArgs
    {
        public Stream Stream { get; set; }
    }
}