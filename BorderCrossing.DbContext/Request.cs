using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.DbContext
{
    public class Request
    {
        public Guid RequestId { get; set; }

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public LocationHistoryFile File { get; set; }

        public List<CheckPoint> CheckPoints { get; set; }
    }
}
