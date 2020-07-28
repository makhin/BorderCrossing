using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorderCrossing.DbContext
{
    public class Request
    {
        public Guid RequestId { get; set; }

        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public LocationHistoryFile File { get; set; }

        public List<CheckPoint> CheckPoints { get; set; }
    }
}
