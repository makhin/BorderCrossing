using System.Collections.Generic;
using System.Linq;

namespace BorderCrossing.Models
{
    public class BorderCrossingResponse
    {
        public List<Period> Periods { get; set; }

        public Dictionary<string, int> CountryDays
        {
            get
            {
                return Periods.GroupBy(p => p.Country)
                    .Select(g => new { Country = g.Key, Days = g.Sum(p => p.Days) })
                    .ToDictionary(p => p.Country, d => d.Days);
            }
        }

        public BorderCrossingResponse()
        {
            Periods = new List<Period>();
        }
    }
}