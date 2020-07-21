using System;
using System.IO;

namespace BorderCrossing.Components
{
    public class FileUploaderBaseEventArgs : EventArgs
    {
        public MemoryStream Stream { get; set; }
    }
}