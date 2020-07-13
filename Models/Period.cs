using System;

namespace BorderCrossing.Models
{
    public class Period
    {
        public CheckPoint ArrivalPoint { get; set; }
        
        public string Country { get; set; }
        
        public CheckPoint DeparturePoint { get; set; }

        public int Days
        {
            get
            {
                var days = (DeparturePoint.Date - ArrivalPoint.Date).Days;
                return days > 0 ? days : 0;
            }
        }
    }
}