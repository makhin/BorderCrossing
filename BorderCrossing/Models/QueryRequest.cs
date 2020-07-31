using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

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
                    Name = "Антарктика",
                    Checked = false
                },
                new Region
                {
                    Id = 2,
                    Name = "Африка",
                    Checked = false
                },
                new Region
                {
                    Id = 9,
                    Name = "Австралия",
                    Checked = false
                },
                new Region
                { 
                    Id = 19,
                    Name = "Америка",
                    Checked = false
                },
                new Region
                {
                    Id = 142,
                    Name = "Азия",
                    Checked = true
                },
                new Region
                {
                    Id = 150,
                    Name = "Европа",
                    Checked = true
                },
            };
        }
    }
}