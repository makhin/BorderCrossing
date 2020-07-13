using System;
using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.DbContext
{
    public class LocationHistoryFile
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime DateUpload { get; set; }
        
        public string FileName { get; set; }
        
        public byte[] File { get; set; }
    }
}