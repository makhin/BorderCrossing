using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossBorders.MVC.Models
{
    public class Results
    {
        public List<Period> Periods { get; set; }

        public Dictionary<string, int> CountryDays
        {
            get
            {
                return Periods.GroupBy(p => p.Country)
                    .Select(g => new {Country = g.Key, Days = g.Sum(p => p.Days)})
                    .ToDictionary(p => p.Country, d => d.Days);
            }
        }

        public Results()
        {
            Periods = new List<Period>();
        }
    }

    public class Period
    {
        public DateTime ArrivalDate { get; set; }
        
        public string Country { get; set; }
        
        public DateTime DepartureDate { get; set; }

        public int Days
        {
            get
            {
                var days = (DepartureDate - ArrivalDate).Days;
                return days > 0 ? days : 0;
            }
        }
    }
}