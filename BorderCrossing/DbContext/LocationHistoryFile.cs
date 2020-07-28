using System;
using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.DbContext
{
    public class LocationHistoryFile
    {
        [Key]
        public Guid RequestId { get; set; }

        public Request Request { get; set; }

        [Required]
        public DateTime DateUpload { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        public byte[] File { get; set; }

    }
}