using System;
using System.ComponentModel.DataAnnotations;

namespace CrossBorders.MVC.DbContext
{
    public class UsageHistory
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime DateUpload { get; set; }
        
        public string FileName { get; set; }
        
        public byte[] File { get; set; }
    }
}