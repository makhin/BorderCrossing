using System;
using System.IO;
using Blazor.FileReader;

namespace BorderCrossing.Components
{
    public class FileUploaderBaseEventArgs : EventArgs
    {
        public MemoryStream Stream { get; set; }
    }
}