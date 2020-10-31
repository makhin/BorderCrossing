using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BorderCrossing.Models
{
    public class QueryRequest
    {
        [DataType(DataType.Date)]
        [Required]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public IntervalType IntervalType { get; set; }

        [Required]
        public List<Region> Regions { get; set; }

        public QueryRequest()
        {
            Regions = new List<Region>()
            {
                new Region
                {
                    Id = 0,
                    Name = "Antarctic",
                    Checked = true
                },
                new Region
                {
                    Id = 2,
                    Name = "Africa",
                    Checked = true
                },
                new Region
                {
                    Id = 9,
                    Name = "Australia",
                    Checked = true
                },
                new Region
                { 
                    Id = 19,
                    Name = "America",
                    Checked = true
                },
                new Region
                {
                    Id = 142,
                    Name = "Asia",
                    Checked = true
                },
                new Region
                {
                    Id = 150,
                    Name = "Europe",
                    Checked = true
                },
            };
        }
    }
}