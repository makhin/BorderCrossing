using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BorderCrossing.Models
{
    public class DateRangePostRequest
    {
        [HiddenInput]
        [Required]
        public string Guid { get; set; }
        
        [DataType(DataType.Date)]
        [Required]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 2)]
        [Required]
        public int Interval { get; set; }

        [Required]
        public List<Region> Regions { get; set; }

        public DateRangePostRequest()
        {
            Regions = new List<Region>()
            {
                new Region
                {
                    Id = 0,
                    Name = "Antarctica",
                    Checked = false
                },
                new Region
                {
                    Id = 2,
                    Name = "Africa",
                    Checked = false
                },
                new Region
                {
                    Id = 9,
                    Name = "Australia",
                    Checked = false
                },
                new Region
                { 
                    Id = 19,
                    Name = "America",
                    Checked = false
                },
                new Region
                {
                    Id = 142,
                    Name = "Asia",
                    Checked = false
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